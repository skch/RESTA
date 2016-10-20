using Newtonsoft.Json.Linq;
using RestUseCases.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases
{
	public class TestSequence
	{
		public static void execute(RunbookStatus rbook, XElement xsq)
		{
			var status = new SequenceStatus();
			status = readSequenceXml(status, xsq);
			status = addCommonData(status, rbook);
			status = runSequence(status);
			displayResult(status);
		}

		// ----------------------------------------------------
		private static SequenceStatus readSequenceXml(SequenceStatus status, XElement xsq)
		{
			if (status.HasErrors) return status;
			try
			{
				// Prepare sequence data template
				status.CasesPath = "?";

				// Create list of contexts
				var xct = xsq.Elements("context");
				if (xct != null) 
					foreach (XElement xcontext in xct)
					{
						string key = XTools.Attr(xcontext, "id");
						status.indexContext[key] = CommonTools.xmlToJson(xcontext.Elements("var"));
					}

				// Create default context
				if (!status.indexContext.ContainsKey("default")) status.indexContext["default"] = new JObject();

				// Load all test cases to memory
				foreach (XElement xtest in xsq.Elements("test"))
				{
					string tid = XTools.Attr(xtest, "src");
					string fname = xtest.Attribute("src").Value + ".xml";
					if (!File.Exists(status.CasesPath + fname)) {
						// return status.setError("File not found: " + fname);
						// Will ignore missing test case files
						Console.WriteLine("Skip test case. File not found: " + fname);
						continue;
					} 
					var xtcase = XDocument.Load(status.CasesPath + fname);
					xtcase.Add(new XAttribute("id", fname));
					var contextAtr = xtest.Attribute("context");
					if (contextAtr != null) xtcase.Add(contextAtr);
					status.Operations.Add(xtcase.Root);
				}
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "run sequence");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus addCommonData(SequenceStatus status, RunbookStatus rbook)
		{
			if (status.HasErrors) return status;
			try
			{
				status.EnvName = "";
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "run sequence");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus runSequence(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				status.XReport = new XElement("report", new XAttribute("id", status.Id), new XAttribute("environment", status.EnvName));
				Console.WriteLine("\n\n# Sequence {0}\n", status.Id);
				foreach (XElement xtest in status.Operations)
				{
					status = TestCase.executeTest(status, xtest);
					if (status.HasErrors && status.toBreakOnFail) return status;
					status.reset(); // remove errors, but keep the context
				}
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "run sequence");
			}
		}

		// ----------------------------------------------------
		private static void displayResult(SequenceStatus status)
		{
			string reportFile = "report-" + status.Id + ".xml";
			if (File.Exists(reportFile)) File.Delete(reportFile);
			var doc = new XDocument(status.XReport);
			doc.Save(reportFile);
			Console.WriteLine();
		}
	}
}
