using Nustache.Core;
using RestUseCases.Rest;
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

		// ----------------------------------------------------
		public static SequenceStatus executeTest(SequenceStatus status, XElement xtest)
		{
			if (status.HasErrors) return status;
			status.currentCase = xtest;
			Console.Write("Running test {0} >", status.Id);

			status = prepareTestData(status, xtest);
			switch (status.rtype)
			{
				case "GET": status = executeRestGet(status); break;
				case "POST": status = executeRestPost(status); break;
				// case "PUT": status = executeRestPost(status); break;
				// case "DELETE": status = executeRestGet(status); break;
				default: Terminal.WriteError("Unsupported test type: " + status.rtype); break;
			}
			status = addStatusReport(status);
			return status;
		}

		// ----------------------------------------------------
		private static SequenceStatus prepareTestData(SequenceStatus status, XElement xtest)
		{
			if (status.HasErrors) return status;
			try
			{
				status.headers = new Dictionary<string, string>();


				status.input = new RestRequest();
				status.input.Url = XTools.Attr(xtest, "url");
				status.input.Url = Render.StringToString(status.input.Url, book.envariables);
				status.input.header = book.headers;
				addHeaders(status.input.header, this.headers);

				xrequest = status.input.AsXml(rtype);

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
				status.Response = client.get(status.rtype, status.input);
				Console.Write(">");
				xresponse = status.Response.AsXml();
				if (!status.Response.Success) return logError("HTTP Error: {0}", status.Response.Message);
				if (status.Response.HttpCode != HttpCode) return logError("HTTP code {0}, expected {1}", status.Response.HttpCode, HttpCode);
				if (status.Response.ContentType != ContentType) return logError("Content type {0}, expected {1}", status.Response.ContentType, ContentType);
				if (status.Response.ContentType.Contains("json"))
				{
					bool isValid = validateJson();
					if (!isValid) return Terminal.WriteWarning("> FAILS: Invalid Response format");
					SaveResult(root);
				}
				Console.WriteLine("> {0} OK", theResponse.Duration);

				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute test");
			}
		}


		// ----------------------------------------------------
		private static SequenceStatus executeRestPost(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			Console.Write("Running test {0} >", tid);
			try
			{
				var input = new RestRequest();
				input.Url = XTools.Attr(root, "url");
				input.Url = Render.StringToString(input.Url, book.envariables);
				JObject templates = book.envariables;
				templates.Merge(book.seqvariables);
				input.Content = root.Element("data").Value;
				RenderContextBehaviour r = new RenderContextBehaviour();
				r.HtmlEncoder = (x) => { return x; };
				input.Content = Render.StringToString(input.Content, JSTools.objectToDict(templates), r);

				input.header = book.headers;

				xrequest = input.AsXml(rtype);
				var client = new RestClient();
				Console.Write("{0}>", rtype);
				theResponse = client.post(rtype, input);
				Console.Write(">");
				xresponse = theResponse.AsXml();
				if (!theResponse.Success) return logError("HTTP Error: {0}", theResponse.Message);
				if (theResponse.HttpCode != HttpCode) return logError("returns code {0} instead of {1}", theResponse.HttpCode, HttpCode);
				if (theResponse.ContentType != ContentType) return logError("content type {0}, expected {1}", theResponse.ContentType, ContentType);

				if (theResponse.ContentType.Contains("json"))
				{
					bool isValid = validateJson();
					if (!isValid) return Terminal.WriteWarning("> FAILS: Invalid Response format");
					SaveResult(root);

				}
				Console.WriteLine("> {0} OK", theResponse.Duration);

				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute test");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus addStatusReport(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				Console.Write(">");
				var xresponse = status.Response.AsXml();
				if (!status.Response.Success) return logError("HTTP Error: {0}", status.Response.Message);
				if (status.Response.HttpCode != HttpCode) return logError("returns code {0} instead of {1}", status.Response.HttpCode, HttpCode);
				if (status.Response.ContentType != ContentType) return logError("content type {0}, expected {1}", status.Response.ContentType, ContentType);

				if (theResponse.ContentType.Contains("json"))
				{
					bool isValid = validateJson();
					if (!isValid) return Terminal.WriteWarning("> FAILS: Invalid Response format");
					SaveResult(root);

				}
				Console.WriteLine("> {0} OK", status.Response.Duration);
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "execute test");
			}
		}



	}
}
