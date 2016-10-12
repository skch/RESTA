#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using skch.rest;
using Nustache.Core;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Rest
{
	public class TestRunBook
	{
		private XDocument xbook;
		private XDocument xenv;
		private string casesPath;
		public XElement xreport;
		public string Environment;
		internal JObject envariables;
		internal JObject seqvariables;
		internal Dictionary<string, string> headers = new Dictionary<string, string>();

		public bool Load(string rbook, string env)
		{
			try
			{
				xbook = loadXmlFile(rbook);
				xenv = loadXmlFile(env);
				if (xbook == null || xenv == null) return false;
				casesPath = Path.GetDirectoryName(rbook);
				if(casesPath.Length > 0) casesPath += Path.DirectorySeparatorChar;

				Environment = XTools.Attr(xenv.Root, "name");
				envariables = new JObject();
				foreach (XElement xvar in xenv.Root.Element("variables").Elements())
				{
					envariables.Add(new JProperty(XTools.Attr(xvar, "id"), XTools.Attr(xvar, "value")));
				}
				foreach (XElement xhead in xenv.Root.Element("header-all").Elements())
				{
					headers.Add(XTools.Attr(xhead, "id"), XTools.Attr(xhead, "value"));
				}
				foreach (XElement xhead in xbook.Root.Element("header").Elements())
				{
					headers.Add(XTools.Attr(xhead, "id"), XTools.Attr(xhead, "value"));
				}
				return true;
			}
			catch (Exception ex)
			{
				Terminal.WriteError(ex);
				return false;
			}
		}

		private XDocument loadXmlFile(string fname)
		{
			try
			{
				if (!File.Exists(fname))
				{
					Console.WriteLine("File {0} does not exists");
					return null;
				}				
				return XDocument.Load(fname);

			} catch (Exception ex)
			{
				Terminal.WriteError("Invalid XML format: {0}\n{1}", fname, ex.Message);
				return null;
			}
		}

		public int Execute()
		{
			int result = 0;
			foreach (XElement xsq in xbook.Root.Elements("sequence"))
			{
				result += executeSequence(xsq);
			}
			return result > 0 ? 1 : result;
		}

		private int executeSequence(XElement xsequence)
		{
			int result = 0;
			string id = XTools.Attr(xsequence, "id");
			string reportFile = "report-" + id + ".xml";
			xreport = new XElement("report", new XAttribute("id", id), new XAttribute("environment", Environment));
			if (File.Exists(reportFile)) File.Delete(reportFile);
			Console.WriteLine("\n\n# Sequence {0}\n", id);
			seqvariables = new JObject();
			foreach(XElement seqvar in xsequence.Elements("var"))
			{
				seqvariables.Add(XTools.Attr(seqvar, "id"), string.Empty);
			}

			foreach (XElement xtest in xsequence.Elements("test"))
			{
				result += executeTest(xtest);
			}
			var doc = new XDocument(xreport);
			doc.Save(reportFile);
			return result;
		}

		private int executeTest(XElement xtest)
		{
			string tid = xtest.Attribute("src").Value;
			string fname =  xtest.Attribute("src").Value + ".xml";
			if (!File.Exists(casesPath + fname))
			{
				Terminal.WriteError("File not found: "+fname);
				return -1;
			}

			var tc = new TestCase(this);
			if (!tc.Load(casesPath + fname)) return -1;
			return tc.Execute(fname);
		}

		private void executeRestGet(string id, XElement xtest)
		{
			Console.WriteLine("Running test "+id);
			try
			{
				var input = new RestRequest();
				input.Url = XTools.Attr(xtest, "url");
				input.Url = Render.StringToString(input.Url, envariables);
				input.header = headers;
				var client = new RestClient();
				var response = client.get("GET", input);
				Console.WriteLine("  url:          {0}", input.Url);
				Console.WriteLine("  code:         {0}", response.HttpCode);
				Console.WriteLine("  content type: {0}", response.ContentType);
				Console.WriteLine("  Message:      {0}", response.Message);
				Console.WriteLine(response.RawData);

			}
			catch (Exception ex)
			{
				Terminal.WriteError(ex);
			}
		}
	}
}
