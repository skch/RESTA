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
	
		//===========================================================
		public bool Execute(ProcessContext context, RestaParams opt, RunBook book)
		{
			if (context.HasErrors) return false;
			Console.WriteLine("Runbook: {0}", book.title);
			var tcase = new RestApiCase
			{
				outputPath = opt.outputPath,
				schemaPath = opt.schemaPath,
				inputPath = opt.inputPath,
				toSaveSuccess = opt.keepSuccess,
				displayLog = opt.verbose,
				includeResponseHeader = opt.responseHeader,
				failFast = opt.failFast
			};

			Console.WriteLine("Environment: {0}", book.environment.title);
			foreach (var scriptData in book.scripts)
			{
				bool success = tcase.Execute(context, book.environment, scriptData);
				if (!success & opt.failFast) break;
			}

			saveEnvironment(context, book.environment, opt.outputPath);
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
		
	
	}
}