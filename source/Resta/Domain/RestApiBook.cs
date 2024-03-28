#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using Newtonsoft.Json;
using Resta.Model;

namespace Resta.Domain
{
	public class RestApiBook
	{
		
		private int countSteps;
		private int countPassed;
		private int countFailed;
	
		//===========================================================
		public bool Execute(ProcessContext context, RestaParams opt, RunBook book)
		{
			if (context.HasErrors) return false;
			Console.WriteLine("Runbook: {0}", book.title);
			countSteps = 0;
			countPassed = 0;
			countFailed = 0;
			var tcase = new RestApiCase
			{
				outputPath = opt.outputPath,
				schemaPath = opt.schemaPath,
				inputPath = opt.inputPath,
				toSaveSuccess = opt.keepSuccess,
				displayLog = opt.verbose,
				includeResponseHeader = opt.responseHeader,
				failFast = opt.failFast,
				ToShowVariables = !opt.hideVariables
			};

			Console.WriteLine("Environment: {0}", book.environment.title);
			foreach (var scriptData in book.scripts)
			{
				countSteps += scriptData.tasks.Count;
				var res = tcase.Execute(context, book.environment, scriptData);
				countFailed += res.Failed;
				countPassed += res.Passed;
				if (!res.Success & opt.failFast) break;
			}

			saveEnvironment(context, book.environment, opt.outputPath);
			writeSummary(context, book);
			return true;
		}
		
		//--------------------------------------------------
		private bool saveEnvironment(ProcessContext context, RestEnvironment environment, string path)
		{
			if (context.HasErrors) return false;
			var fullname = Path.Combine(path, "env-" + environment.id + ".json");
			try
			{
				string json = JsonConvert.SerializeObject(environment, Formatting.Indented); 
				File.WriteAllText(fullname, json);
				return true;
			}
			catch (Exception ex)
			{
				return context.SetError(false, "Saving environment "+environment.id, ex);
			}
		}
		
		//--------------------------------------------------
		private bool writeSummary(ProcessContext context, RunBook book)
		{
			if (context.HasErrors) return false;
			Console.WriteLine();
			Console.WriteLine($"SUMMARY: Tests: {book.scripts.Count}, total steps: {countSteps}, passed: {countPassed}, failed: {countFailed}.");
			return true;
		}
		
	
	}
}