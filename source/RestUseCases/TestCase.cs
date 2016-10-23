#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Nustache.Core;
using RestUseCases.Domain;
using RestUseCases.Rest;
using RestUseCases.Tools;
using skch.rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases
{
	public class TestCase
	{

		// =================================================
		public static SequenceStatus executeTest(SequenceStatus status, TaskMetadata xtest)
		{
			if (status.HasErrors) return status;
			status.currentCase = xtest;
			Console.Write("Running test {0} >", status.currentCase.Id);

			status = prepareTestData(status, xtest);
			Console.Write("{0}>", status.currentCase.rtype);
			switch (status.currentCase.rtype)
			{
				case "GET": status = executeRestGet(status); break;
				case "POST": status = executeRestPost(status); break;
				// case "PUT": status = executeRestPost(status); break;
				// case "DELETE": status = executeRestGet(status); break;
				default: Terminal.WriteError("Unsupported test type: " + status.currentCase.rtype); break;
			}
			Console.Write(">");
			status = validateResponse(status);
			status = updateContext(status);
			status = finalizeTestCase(status);
			return status;
		}


		// ----------------------------------------------------
		private static SequenceStatus prepareTestData(SequenceStatus status, TaskMetadata xtest)
		{
			if (status.HasErrors) return status;
			try
			{
				status.xTestReport = new XElement("test-case", new XAttribute("id", status.metadata.Id));
				status.headers = new Dictionary<string, string>();

				// Prepare data to render input
				var listVariables = JSTools.objectToDict(status.context);
				// RenderContextBehaviour render = new RenderContextBehaviour();
				// render.HtmlEncoder = (x) => { return x; };
				status.input = new RestRequest();

				// Render request URL
				status.input.Url = xtest.Url;
				status.input.Url = Render.StringToString(status.input.Url, listVariables);

				// Render content
				status.input.Content = status.currentCase.Data;
				status.input.Content = Render.StringToString(status.input.Content, listVariables);

				// Build list of headers and render them
				var headerTmp = new Dictionary<string, string>();
				CommonTools.dictMerge(headerTmp, status.headers);
				CommonTools.dictMerge(headerTmp, xtest.Headers);
				var headers = new Dictionary<string, string>();
				foreach (string key in headerTmp.Keys)
				{
					string txt = headerTmp[key];
					headers.Add(key, Render.StringToString(txt, listVariables));
				}
				status.input.header = headers;

				// Add the request to the XML report
				var xrequest = status.input.AsXml(status.currentCase.rtype);
				status.xTestReport.Add(xrequest);

				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute test");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus executeRestGet(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				var client = new RestClient();
				status.Response = client.get(status.currentCase.rtype, status.input);
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute GET request");
			}
		}


		// ----------------------------------------------------
		private static SequenceStatus executeRestPost(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				var client = new RestClient();
				status.Response = client.post(status.currentCase.rtype, status.input);
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute POST request");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus validateResponse(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				// Add response to the test report
				var xresponse = status.Response.AsXml();
				status.xTestReport.Add(xresponse);

				if (!status.Response.Success) return status.setError("HTTP Error: "+status.Response.Message);
				if (status.Response.HttpCode != status.currentCase.HttpCode)
					return status.setWarning(String.Format("returns code {0} instead of {1}", status.Response.HttpCode, status.currentCase.HttpCode));

				if (status.Response.ContentType != status.currentCase.ContentType)
					return status.setWarning(String.Format("content type {0}, expected {1}", status.Response.ContentType, status.currentCase.ContentType));

				if (!status.Response.ContentType.Contains("json")) return status; // Only JSON response will be validated

				var jt = JToken.Parse(status.Response.RawData);
				status.Response.CleanData = jt.ToString(Formatting.Indented);
				xresponse.Add(new XElement("content", status.Response.CleanData));
				xresponse.Element("data").Remove();

				var jresponse = JToken.Parse(status.Response.CleanData);
				IList<ValidationError> schemaErrors;
				bool valid = jresponse.IsValid(status.currentCase.schema, out schemaErrors);
				foreach (ValidationError er in schemaErrors)
				{
					xresponse.Add(new XElement("jerror",
						new XAttribute("msg", er.Message),
						new XAttribute("line", er.LineNumber),
						new XAttribute("col", er.LinePosition)
						));
				}

				if (valid) return status;
				return status.setWarning("Invalid response format");
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute test");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus updateContext(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				string cname = status.currentCase.ContextVariable;
				if (String.IsNullOrEmpty(cname)) return status; // no need
				var jt = JToken.Parse(status.Response.RawData);
				status.context.Add(new JProperty(cname, jt));
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "update context");
			}
		}


		// ----------------------------------------------------
		private static SequenceStatus finalizeTestCase(SequenceStatus status)
		{
			try
			{
				status.XReport.Add(status.xTestReport);
				switch (status.Result)
				{
					case 0: Console.WriteLine(    "> {0} OK", status.Response.Duration); break;
					case 1: Terminal.WriteWarning("> FAILS: "+status.errorMessage); break;
					case 2: Terminal.WriteError(  "> ERROR: " + status.errorMessage); break;
					default: Terminal.WriteError( "> CRACH: " + status.errorMessage); break;
				}
				return status;
			}
			catch (Exception ex)
			{
				Terminal.WriteError("> CRACH: " + ex.Message);
				return status.setException(ex, "display result");
			}
		}



	}
}
