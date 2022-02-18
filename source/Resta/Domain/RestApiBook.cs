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
				tcase.Execute(context, env, scriptData);
			}
			
			
			return true;
		}
		
		//--------------------------------------------------
		public static RestEnvironment loadEnvironment(ProcessContext context, string path, string fname)
		{
			if (context.HasErrors) return null;
			var fullname = Path.Combine(path, "env-" + fname + ".json");
			if (!File.Exists(fullname)) return context.SetError<RestEnvironment>(null, "Cannot find environment file");
			try
			{
				string json = File.ReadAllText(fullname);			
				RestEnvironment res = JsonConvert.DeserializeObject<RestEnvironment>(json);
				
				return res;
			}
			catch (Exception ex)
			{
				return context.SetError<RestEnvironment>(null, "Load Environment", ex);
			}
		}

		//--------------------------------------------------
		private RestScript loadScriptData(ProcessContext context, string path, string fname)
		{
			if (context.HasErrors) return null;
			var fullname = Path.Combine(path, "script-" + fname + ".json");
			if (!File.Exists(fullname)) return context.SetError<RestScript>(null, "Cannot find script "+fname);
			try
			{
				string json = File.ReadAllText(fullname);
				RestScript res = JsonConvert.DeserializeObject<RestScript>(json);
				return res;
			}
			catch (Exception ex)
			{
				return context.SetError<RestScript>(null, "Load script "+fname, ex);
			}
		}
	}
}