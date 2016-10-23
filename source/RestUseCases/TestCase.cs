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
			status.TestCaseMd = xtest;
			Console.Write("Running test {0} >", status.TestCaseMd.Id);

			status = prepareTestData(status, xtest);
			Console.Write("{0}>", status.TestCaseMd.rtype);
			switch (status.TestCaseMd.rtype)
			{
				case "GET": status = executeRestGet(status); break;
				case "POST": status = executeRestPost(status); break;
				// case "PUT": status = executeRestPost(status); break;
				// case "DELETE": status = executeRestGet(status); break;
				default: Terminal.WriteError("Unsupported test type: " + status.TestCaseMd.rtype); break;
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
				status.XmlCaseReport = new XElement("test-case", new XAttribute("id", status.TestCaseMd.Id));

				// Prepare data to render input
				var listVariables = JSTools.objectToDict(status.Context);
				// RenderContextBehaviour render = new RenderContextBehaviour();
				// render.HtmlEncoder = (x) => { return x; };
				status.RestInput = new RestRequest();

				// Render request URL
				status.RestInput.Url = xtest.Url;
				status.RestInput.Url = Render.StringToString(status.RestInput.Url, listVariables);

				// Render content
				status.RestInput.Content = status.TestCaseMd.Data;
				status.RestInput.Content = Render.StringToString(status.RestInput.Content, listVariables);

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
				status.RestInput.header = headers;

				// Add the request to the XML report
				var xrequest = status.RestInput.AsXml(status.TestCaseMd.rtype);
				status.XmlCaseReport.Add(xrequest);

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
				status.RestOutput = client.get(status.TestCaseMd.rtype, status.RestInput);
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
				status.RestOutput = client.post(status.TestCaseMd.rtype, status.RestInput);
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
				var xresponse = status.RestOutput.AsXml();
				status.XmlCaseReport.Add(xresponse);

				if (!status.RestOutput.Success) return status.setError("HTTP Error: "+status.RestOutput.Message);
				if (status.RestOutput.HttpCode != status.TestCaseMd.HttpCode)
					return status.setWarning(String.Format("returns code {0} instead of {1}", status.RestOutput.HttpCode, status.TestCaseMd.HttpCode));

				if (status.RestOutput.ContentType != status.TestCaseMd.ContentType)
					return status.setWarning(String.Format("content type {0}, expected {1}", status.RestOutput.ContentType, status.TestCaseMd.ContentType));

				if (!status.RestOutput.ContentType.Contains("json")) return status; // Only JSON response will be validated

				var jt = JToken.Parse(status.RestOutput.RawData);
				status.RestOutput.CleanData = jt.ToString(Formatting.Indented);
				xresponse.Add(new XElement("content", status.RestOutput.CleanData));
				xresponse.Element("data").Remove();

				var jresponse = JToken.Parse(status.RestOutput.CleanData);
				IList<ValidationError> schemaErrors;
				bool valid = jresponse.IsValid(status.TestCaseMd.schema, out schemaErrors);
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
				string cname = status.TestCaseMd.ContextVariable;
				if (String.IsNullOrEmpty(cname)) return status; // no need
				var jt = JToken.Parse(status.RestOutput.RawData);
				status.Context.Add(new JProperty(cname, jt));
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
				if (status.Result > 0) status.XmlSequenceReport.Add(status.XmlCaseReport);
				switch (status.Result)
				{
					case 0: Console.WriteLine(    "> {0} OK", status.RestOutput.Duration); break;
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
