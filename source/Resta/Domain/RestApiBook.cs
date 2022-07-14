#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters;
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
				OutputPath = opt.OutputPath,
				SchemaPath = opt.SchemaPath,
				InputPath = opt.InputPath,
				ToSaveSuccess = opt.KeepSuccess,
				DisplayLog = opt.Verbose,
				IncludeResponseHeader = opt.ResponseHeader,
				FailFast = opt.FailFast
			};
			/*
			var list = new List<RestScript>();
			foreach (var scriptName in book.scripts)
			{
				var scriptData = loadScriptData(context, opt.ScriptPath, scriptName);
				tcase.Validate(context, env, scriptData);
				list.Add(scriptData);
			}
			*/
			
			
			Console.WriteLine("Environment: {0}", book.environment.title);
			foreach (var scriptData in book.scripts)
			{
				bool success = tcase.Execute(context, book.environment, scriptData);
				if (!success & opt.FailFast) break;
			}
			
			
			return true;
		}
		
		

		
		//--------------------------------------------------
		/*
		private static RestEnvironment loadEnvironment(ProcessContext context, string path, string fname)
		{
			RestEnvironment res = new RestEnvironment();
			if (context.HasErrors) return res;
			var fullname = Path.Combine(path, "env-" + fname + ".json");
			if (!File.Exists(fullname)) return context.SetError<RestEnvironment>(res, "Cannot find environment file");
			try
			{
				string json = File.ReadAllText(fullname);			
				var env = JsonConvert.DeserializeObject<RestEnvironment>(json);
				if (env == null) return context.SetError(res, "Cannot load environment JSON");
				return env;
			}
			catch (Exception ex)
			{
				return context.SetError(res, "Load Environment", ex);
			}
		}
		*/

		//--------------------------------------------------
		/*
		private RestScript loadScriptData(ProcessContext context, string path, string fname)
		{
			RestScript res = new RestScript();
			if (context.HasErrors) return res;
			var fullname = Path.Combine(path, "script-" + fname + ".json");
			if (!File.Exists(fullname)) return context.SetError<RestScript>(res, "Cannot find script "+fname);
			try
			{
				string json = File.ReadAllText(fullname);
				var script = JsonConvert.DeserializeObject<RestScript>(json);
				if (script == null) return context.SetError(res, "Cannot load script JSON");
				return script;
			}
			catch (Exception ex)
			{
				return context.SetError(res, "Load script "+fname, ex);
			}
		}
		*/
	}
}