#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using RestUseCases.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases
{
	class TestRunbook
	{
		static int Main(string[] args)
		{
			Console.WriteLine("REST API Command Line Testing Tool");
			var status = new RunbookStatus();
			status = readCmdParameters(status, args);
			status = loadMetadata(status);
			status = validateRunbook(status);
			status = executeRunbook(status);
			displayResult(status);

#if DEBUG
			Console.WriteLine("Press [ENTER] to exit");
			Console.ReadLine();
#endif
			return status.Result;
		}

		private static string getParam(string[] plist, int pn, string value)
		{
			if (plist.Length > pn - 1) return plist[pn - 1];
			return value;
		}

		// ----------------------------------------------------
		private static RunbookStatus readCmdParameters(RunbookStatus status, string[] args)
		{
			if (status.HasErrors) return status;
			status.BookFileName = getParam(args, 1, "runbook.xml");
			status.EnvironmentName = getParam(args, 2, "environment.xml");
			return status;
		}

		// ----------------------------------------------------
		private static RunbookStatus loadMetadata(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				if (!File.Exists(status.EnvironmentName)) return status.setError("File does not exists: "+ status.EnvironmentName);
				var edoc = XDocument.Load(status.EnvironmentName);
				if (!File.Exists(status.BookFileName)) return status.setError("File does not exists: " + status.BookFileName);
				var doc = XDocument.Load(status.BookFileName);
				status.BookMd = new RunbookMetadata(doc.Root, edoc.Root);
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "load metadata");
			}
		}

		// ----------------------------------------------------
		private static RunbookStatus validateRunbook(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				string emsg = status.BookMd.Validate();
				if (!String.IsNullOrEmpty(emsg)) return status.setError("Invalid test metadata: " + emsg);
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "validate runbook");
			}
		}

		// ----------------------------------------------------
		private static RunbookStatus executeRunbook(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				foreach (XElement xsq in status.BookMd.Sequence)
				{
					TestSequence.execute(status, xsq);
					status.TestCount++;
				}
				return status;
			} catch (Exception ex)
			{
				return status.setException(ex, "execute runbook");
			}
		}

		// ----------------------------------------------------
		private static void displayResult(RunbookStatus status)
		{
			switch (status.TestCount)
			{
				case 0: Console.WriteLine("No test sequences found"); break;
				case 1: Console.WriteLine("The test sequence is completed"); break;
				default: Console.WriteLine("Completed {0} test sequences", status.TestCount); break;
			}
		}

		
	}
}
