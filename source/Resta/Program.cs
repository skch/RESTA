#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using Resta.Domain;
using Resta.Model;

namespace Resta
{
	static class Program
	{
	
		static string AppVersion = "1.2.04";
		static void Main(string[] args)
		{
			FluentConsole
				.Text("REST API Automated Testing").Cyan.Line(" v"+AppVersion);
			var context = new ProcessContext();
			var cparams = getProcessParams(context, args);
			if (cparams.needHelp)
			{
				help(); return;
			}
			
			if (cparams.createNewBook)
			{
				var scriptBuilder = new ScriptBuilder();
				scriptBuilder.buildRunbook(context, cparams);

			} else
			{
				
				Console.WriteLine("Loading scripts...");
				var dataValidator = new RestaScriptValidator();
				dataValidator.LoadRunbook(context, cparams);
				var runbook = dataValidator.ValidateScripts(context);

				var book = new RestApiBook();
				var start = DateTime.Now;
				book.Execute(context, cparams, runbook);
				var time = DateTime.Now - start;
			
				Console.WriteLine();
				FluentConsole
					.Text("API Test ")
					.With(c => !context.HasErrors ? c.Green : c.Red)
					.Line(!context.HasErrors ? "Completed" : "Failed");
				
				Console.WriteLine("Total Duration: {0}", time);
			}

			if (context.HasErrors)
			{
				Console.WriteLine(context.ErrorMessage);
			}
		}
		
		//--------------------------------------------------
		static RestaParams getProcessParams(ProcessContext context, string[] args)
		{
			var res = new RestaParams();
			if (context.HasErrors) return res;
			if (args.Length == 0)
			{
				res.needHelp = true;
				return res;
			}

			foreach (var cp in args)
			{
				if (cp.StartsWith('-'))
				{
					string pair = cp.Substring(1, cp.Length - 1);

					(string key, string value) = splitCmdOption(pair);
					switch (key.ToLower())
					{
						case "env": res.environmentName = value; break;
						case "sc": res.schemaPath = value; break;
						case "out": res.outputPath = value; break;
						case "in": res.inputPath = value; break;
						case "keep": res.keepSuccess = true; break;
						case "debug": res.verbose = true; break;
						case "rh": res.responseHeader = true; break;
						case "ff": res.failFast = true; break;
						case "new": res.createNewBook = true; break;
						default: res.needHelp = true; break;
					}
				}
				else
				{
					res.bookName = cp;
				}

				var scriptPath = Path.GetDirectoryName(res.bookName);
				res.scriptPath = (string.IsNullOrEmpty(scriptPath)) ? "": scriptPath;
			}
			return res;
		}

		private static (string, string) splitCmdOption(string cmdoption)
		{
			if (string.IsNullOrEmpty(cmdoption)) return ("","");
			var parts = cmdoption.Split(':');
			string key = parts[0];
			string tail = (parts.Length>1) ? cmdoption.Substring(key.Length+1) : "";
			return (key, tail);
		}

		//--------------------------------------------------
		static void help()
		{
			Console.WriteLine("USAGE:");
			Console.WriteLine("resta runbook {options}");
			Console.WriteLine("");
			Console.WriteLine("Options:");
			Console.WriteLine(" -new          Generate sample runbook and scripts");
			Console.WriteLine(" -in:{path}    Define Path for input data");
			Console.WriteLine(" -out:{path}   Define Path to output the results");
			Console.WriteLine(" -sc:{path}    Define Path for schemas");
			Console.WriteLine(" -keep         Save the result even when passed the test");
			Console.WriteLine(" -rh           Include response header");
			Console.WriteLine(" -ff           Stop script execution after first error");
			
		}
	}
}
