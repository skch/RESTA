#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System;
using System.IO;
using Newtonsoft.Json;
using Resta.Domain;
using Resta.Model;

namespace Resta
{
	class Program
	{
	
		static string AppVersion = "1.1.13";
		static void Main(string[] args)
		{
			FluentConsole
				.Text("REST API Automated Testing").Cyan.Line(" v"+AppVersion);
			var context = new ProcessContext();
			var cparams = getProcessParams(context, args);
			if (cparams.NeedHelp)
			{
				help(cparams.Cmd); return;
			}

			var bookData = loadRunBook(context, cparams);
			var book = new RestApiBook();
			var start = DateTime.Now;
			book.Execute(context, cparams.EnvironmentName, cparams, bookData);
			var time = DateTime.Now - start;
			
			Console.WriteLine();
			FluentConsole
				.Text("API Test ")
				.With(c => !context.HasErrors ? c.Green : c.Red)
				.Line(!context.HasErrors ? "Completed" : "Failed");
				
			Console.WriteLine("Total Duration: {0}", time);

			if (context.HasErrors)
			{
				Console.WriteLine(context.ErrorMessage);
			}
		}
		
		//--------------------------------------------------
		static RunBook loadRunBook(ProcessContext context, RestaParams cparams)
		{
			if (context.HasErrors) return null;
			if (!File.Exists(cparams.BookName)) return context.SetError<RunBook>(null, "Cannot find runbook file");
			try
			{
				string json = File.ReadAllText(cparams.BookName);
				RunBook res = JsonConvert.DeserializeObject<RunBook>(json);
				return res;
			}
			catch (Exception ex)
			{
				return context.SetError<RunBook>(null, "Load Runbook", ex);
			}
			
		}
		
		//--------------------------------------------------
		static RestaParams getProcessParams(ProcessContext context, string[] args)
		{
			if (context.HasErrors) return null;
			var res = new RestaParams();
			if (args.Length == 0)
			{
				res.NeedHelp = true;
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
						case "env": res.EnvironmentName = value; break;
						case "sc": res.SchemaPath = value; break;
						case "out": res.OutputPath = value; break;
						case "in": res.InputPath = value; break;
						case "keep": res.KeepSuccess = true; break;
						case "debug": res.Verbose = true; break;
						case "rh": res.ResponseHeader = true; break;
						case "ff": res.FailFast = true; break;
						default: res.NeedHelp = true; break;
					}
				}
				else
				{
					res.BookName = cp;
				}

				res.ScriptPath = Path.GetDirectoryName(res.BookName);
			}
			//if (string.IsNullOrEmpty(res.Cmd)) res.NeedHelp = true;
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
		static void help(string command)
		{

			switch (command)
			{
				case "b":
					Console.WriteLine("");
					break;
					
				default:
					Console.WriteLine("USAGE:");
					Console.WriteLine("resta runbook {options}");
					Console.WriteLine("");
					Console.WriteLine("Options:");
					Console.WriteLine(" -in:{path}    Define Path for input data");
					Console.WriteLine(" -out:{path}   Define Path to output the results");
					Console.WriteLine(" -sc:{path}    Define Path for schemas");
					Console.WriteLine(" -keep         Save the result even when passed the test");
					Console.WriteLine(" -rh           Include response header");
					Console.WriteLine(" -ff           Stop script execution after first error");
					break;
			}
			

		
		}
	}
}
