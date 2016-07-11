﻿#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using nielsen.se.rest;
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
		public XElement xreport;
		public string Environment;
		internal JObject envariables;
		internal Dictionary<string, string> headers = new Dictionary<string, string>();

		public void Load(string rbook, string env)
		{
			try
			{
				xbook = XDocument.Load(rbook);
				xenv = XDocument.Load(env);
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
			}
			catch (Exception ex)
			{
				Terminal.WriteError(ex);
			}
		}

		public void Execute()
		{
			foreach (XElement xsq in xbook.Root.Elements("sequence"))
			{
				executeSequence(xsq);
			}
		}

		private void executeSequence(XElement xsequence)
		{
			string id = XTools.Attr(xsequence, "id");
			string reportFile = "report-" + id + ".xml";
			xreport = new XElement("report", new XAttribute("id", id), new XAttribute("environment", Environment));
			if (File.Exists(reportFile)) File.Delete(reportFile);
			Console.WriteLine("\n\n# Sequence {0}\n", id);
			foreach (XElement xtest in xsequence.Elements("test"))
			{
				executeTest(xtest);
			}
			var doc = new XDocument(xreport);
			doc.Save(reportFile);

		}

		private void executeTest(XElement xtest)
		{
			string tid = xtest.Attribute("src").Value;
			string fname = xtest.Attribute("src").Value + ".xml";
			if (!File.Exists(fname))
			{
				Terminal.WriteError("File not found: "+fname);
				return;
			}

			var tc = new TestCase(this);
			if (!tc.Load(fname)) return;
			tc.Execute(fname);
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
