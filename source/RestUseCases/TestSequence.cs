using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestUseCases.Domain;
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
			status = saveContext(status);
			displayResult(status);
		}

		// ----------------------------------------------------
		private static SequenceStatus readSequenceXml(SequenceStatus status, XElement xsq)
		{
			if (status.HasErrors) return status;
			try
			{
				// Prepare sequence data template
				status.metadata = new SequenceMetadata(xsq);

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
					var md = new TaskMetadata(xtest, xtcase.Root);
					status.Operations.Add(md);
				}
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "read sequence metadata");
			}
		}

		// ----------------------------------------------------
		private static SequenceStatus addCommonData(SequenceStatus status, RunbookStatus rbook)
		{
			if (status.HasErrors) return status;
			try
			{
				status.context = new JObject();
				JSTools.appendDict(status.context, rbook.Book.Data);
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
				status.XReport = new XElement("report", new XAttribute("id", status.metadata.Id), new XAttribute("environment", status.EnvName));
				Console.WriteLine("\n\n# Sequence {0}\n", status.metadata.Id);
				foreach (TaskMetadata xtest in status.Operations)
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
		private static SequenceStatus saveContext(SequenceStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				if (!status.metadata.toSaveContext) return status;
				string jcontext = status.context.ToString(Formatting.Indented);
				status.XReport.Add(new XElement("context", jcontext));
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "save context");
			}
		}

		// ----------------------------------------------------
		private static void displayResult(SequenceStatus status)
		{
			string reportFile = "report-" + status.metadata.Id + ".xml";
			if (File.Exists(reportFile)) File.Delete(reportFile);
			var doc = new XDocument(status.XReport);
			doc.Save(reportFile);
			Console.WriteLine();
		}
	}
}
