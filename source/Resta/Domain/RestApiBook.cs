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
		public bool Execute(ProcessContext context, string ename, RestaParams opt, RunBook book)
		{
			if (context.HasErrors) return false;
			Console.WriteLine("Runbook: {0}", book.title);
			if (book.scripts == null) return context.SetError(false, "Book scripts is missing");
			if (!string.IsNullOrEmpty(ename)) book.environment = ename;
			if (book.environment == null) return context.SetError(false, "Book environment is missing");
			if (string.IsNullOrEmpty(ename)) ename = book.environment;
			var env = loadEnvironment(context, opt.ScriptPath, ename);
			if (context.HasErrors) return false;

			var tcase = new RestApiCase();
			tcase.OutputPath = opt.OutputPath;
			tcase.SchemaPath = opt.SchemaPath;
			tcase.InputPath = opt.InputPath;
			tcase.ToSaveSuccess = opt.KeepSuccess;
			tcase.DisplayLog = opt.Verbose;
			tcase.IncludeResponseHeader = opt.ResponseHeader;
			tcase.FailFast = opt.FailFast;
			var list = new List<RestScript>();
			foreach (var scriptName in book.scripts)
			{
				var scriptData = loadScriptData(context, opt.ScriptPath, scriptName);
				tcase.Validate(context, env, scriptData);
				list.Add(scriptData);
			}
			
			Console.WriteLine("Environment: {0}", env.title);
			foreach (var scriptData in list)
			{
				bool success = tcase.Execute(context, env, scriptData);
				if (!success & opt.FailFast) break;
			}
			
			
			return true;
		}
		
		//--------------------------------------------------
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

		//--------------------------------------------------
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
	}
}