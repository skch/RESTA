#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using nielsen.se.rest;
using NLog;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Rest
{
	public class TestCase
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public TestRunBook book = null;
		private XDocument xdoc;
		private int HttpCode;
		private string ContentType;
		JSchema schema;
		public IList<ValidationError> schemaErrors;
		private XElement xrequest;
		private XElement xresponse;
		private RestResponse theResponse = null;

		public TestCase(TestRunBook parent)
		{
			book = parent;
		}

		public bool Load(string fname)
		{
			try
			{
				xdoc = XDocument.Load(fname);
				var xresult = xdoc.Root.Element("result");
				string codes = XTools.Attr(xresult, "code");
				HttpCode = Convert.ToInt32(codes);
				ContentType = XTools.Attr(xresult, "type");
				schema = JSchema.Parse(xresult.Value);
				return true;
			}
			catch (Exception ex)
			{
				Terminal.WriteError("Invalid XML file:{0}\n{1}", fname, ex.Message);
				return false;
			}

		}

		// ==============================================================
		public int Execute(string tid)
		{
			int result = 0;
			string rtype = xdoc.Root.Attribute("type").Value;
			bool success = false;
			switch (rtype)
			{
				case "GET": success = executeRestGet(tid, rtype, xdoc.Root); break;
				case "POST": success = executeRestPost(tid, rtype, xdoc.Root); break;
				case "PUT": success = executeRestPost(tid, rtype, xdoc.Root); break;
				case "DELETE": success = executeRestGet(tid, rtype, xdoc.Root); break;
				default: Terminal.WriteError("Unsupported test type: " + rtype); break;
			}

			if (!success)
			{
				book.xreport.Add(new XElement("test-case",
					new XAttribute("id",tid), xrequest, xresponse
					));
				result = 1;
			}
			return result;
		}

		// ------------------------------------------------------
		private bool executeRestGet(string tid, string rtype, XElement root)
		{
			Console.Write("Running test {0} >", tid);
			try
			{
				var input = new RestRequest();
				input.Url = XTools.Attr(xdoc.Root, "url");
				input.Url = Render.StringToString(input.Url, book.envariables);
				input.header = book.headers;

				xrequest = input.AsXml(rtype);
				var client = new RestClient();
				Console.Write("{0}>", rtype);
				theResponse = client.get(rtype, input);
				Console.Write(">");
				xresponse = theResponse.AsXml();
				if (!theResponse.Success) return logError("HTTP Error: {0}", theResponse.Message);
				if (theResponse.HttpCode != HttpCode) return logError("HTTP code {0}, expected {1}", theResponse.HttpCode, HttpCode);
				if (theResponse.ContentType != ContentType) return logError("Content type {0}, expected {1}", theResponse.ContentType, ContentType);
				if (theResponse.ContentType.Contains("json"))
				{
					bool isValid = validateJson();
					if (!isValid) return Terminal.WriteWarning("> FAILS: Invalid Response format");
				}
				Console.WriteLine("> {0} OK", theResponse.Duration);
				return true;

			}
			catch (Exception ex)
			{
				addExceptionToReport(ex);
				return Terminal.WriteError("> FAILS: System Error");
			}
		}

		// ------------------------------------------------------
		private bool logError(string msg, params object[] data)
		{
			xresponse.Add(new XElement("error", new XAttribute("msg", String.Format(msg,data))));
			return Terminal.WriteError("> FAILS: "+msg, data);
		}

		// ------------------------------------------------------
		private bool executeRestPost(string tid, string rtype, XElement root)
		{
			Console.Write("Running test {0} >", tid);
			try
			{
				var input = new RestRequest();
				input.Url = XTools.Attr(root, "url");
				input.Url = Render.StringToString(input.Url, book.envariables);

				input.Content = root.Element("data").Value;
				input.Content = Render.StringToString(input.Content, book.envariables);
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
				}
				Console.WriteLine("> {0} OK", theResponse.Duration);
				return true;

			}
			catch (Exception ex)
			{
				addExceptionToReport(ex);
				return Terminal.WriteError("> FAILS: System Error");
			}
		}



		// ------------------------------------------------------
		private void addExceptionToReport(Exception ex)
		{
			xresponse.Add(new XElement("sys-error",
						new XAttribute("msg", ex.Message),
						ex.StackTrace
						));
		}

		// ------------------------------------------------------
		private bool validateJson()
		{
			try
			{
				var jt = JToken.Parse(theResponse.RawData);
				theResponse.CleanData = jt.ToString(Formatting.Indented);
				xresponse.Add(new XElement("content", theResponse.CleanData));
				xresponse.Element("data").Remove();
	
				var jresponse = JToken.Parse(theResponse.CleanData);
				bool valid = jresponse.IsValid(schema, out schemaErrors);

				foreach (ValidationError er in schemaErrors)
				{
					xresponse.Add(new XElement("jerror",
						new XAttribute("msg", er.Message),
						new XAttribute("line", er.LineNumber),
						new XAttribute("col", er.LinePosition)
						));
				}
				return valid;
			} catch (Exception ex)
			{
				addExceptionToReport(ex);
				return Terminal.WriteError("> FAILS: System Error");
			}
		}

		


	}
}
