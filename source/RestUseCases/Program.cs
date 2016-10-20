#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
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
	class Program
	{
		static int Main(string[] args)
		{
			Console.WriteLine("REST API Command Line Testing Tool");
			var status = new RunbookStatus();
			status = readCmdParameters(status, args);
			status = loadEnvironmentConfig(status);
			status = loadRunbookConfig(status);
			status = validateRunbook(status);
			status = executeRunbook(status);
			displayResult(status);

#if DEBUG
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
		private static RunbookStatus loadEnvironmentConfig(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				if (!File.Exists(status.EnvironmentName)) return status.setError("File does not exists: "+ status.EnvironmentName);
				var doc = XDocument.Load(status.EnvironmentName);
				status.Env = doc.Root;
				status.EnvironmentName = XTools.Attr(status.Env, "name");
				status.envariables = CommonTools.xmlToJson(status.Env.Element("variables"));
				status.listHeaders = CommonTools.xmlToDict(status.Env.Element("header-all"));
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "load environment");
			}
		}

		// ----------------------------------------------------
		private static RunbookStatus loadRunbookConfig(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				if (!File.Exists(status.BookFileName)) return status.setError("File does not exists: " + status.BookFileName);
				var doc = XDocument.Load(status.BookFileName);
				status.Book = doc.Root;
				return status;
			}
			catch (Exception ex)
			{
				return status.setException(ex, "load runbook");
			}
		}

		// ----------------------------------------------------
		private static RunbookStatus validateRunbook(RunbookStatus status)
		{
			if (status.HasErrors) return status;
			try
			{
				if (status.Book.Elements("sequence") == null) return status.setError("Missing the sequence element");
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
				foreach (XElement xsq in status.Book.Elements("sequence"))
				{
					TestSequence.execute(status, xsq);
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
			Console.WriteLine("!");
		}

		
	}
}
