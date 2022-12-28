using Newtonsoft.Json;
using Resta.Model;

namespace Resta.Domain;

public class RestaScriptValidator
{
	private RunbookJson? _bookData;
	private RestEnvironmentJson? _envData;
	private readonly List<RestScriptJson> _scripts = new List<RestScriptJson>();
	
	#region Load Scripts
	
	public bool LoadRunbook(ProcessContext context, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (options.isScript)
		{
			if (string.IsNullOrEmpty(options.bookName)) return context.SetError(false, "Script name is missing");
			_bookData = loadVirtualRunbookFile(context, options.bookName);
		} else
		{
			if (string.IsNullOrEmpty(options.bookName)) return context.SetError(false, "Runbook name is missing");
			_bookData = loadRunbookFile(context, options.bookName);
		}
		setupEnvironment(context, options);
		loadRunbookScripts(context, options);
		return loadScriptsData(context, options);
	}
	
	
	//--------------------------------------------------
	private bool setupEnvironment(ProcessContext context, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (_bookData ==null) return context.SetError(false, "Runbook is unavailable");
		if (!string.IsNullOrEmpty(options.environmentName)) _bookData.environment = options.environmentName;
		return true;
	}


	
	//--------------------------------------------------
	private RunbookJson? loadRunbookFile(ProcessContext context, string bookName)
	{
		if (context.HasErrors) return null;
		
		if (!File.Exists(bookName)) return context.SetErrorNull<RunbookJson>( "Cannot find runbook file");
		try
		{
			string json = File.ReadAllText(bookName);
			var book = JsonConvert.DeserializeObject<RunbookJson>(json);
			return book ?? context.SetErrorNull<RunbookJson>("Cannot read runbook JSON");
		}
		catch (Exception ex)
		{
			return context.SetErrorNull<RunbookJson>("Load Runbook", ex);
		}
			
	}
	
	//--------------------------------------------------
	private RunbookJson? loadVirtualRunbookFile(ProcessContext context, string scriptName)
	{
		if (context.HasErrors) return null;
		try
		{
			string json = "{\"title\": \"Single script\",\"scripts\": [\""+scriptName+"\"]}";
			var book = JsonConvert.DeserializeObject<RunbookJson>(json);
			return book ?? context.SetErrorNull<RunbookJson>("Cannot parse runbook JSON");
		}
		catch (Exception ex)
		{
			return context.SetErrorNull<RunbookJson>("Load Runbook", ex);
		}
			
	}
	
	//--------------------------------------------------
	private RestEnvironmentJson? loadEnvironmentFile(ProcessContext context, string path)
	{
		if (context.HasErrors) return null;
		if (_bookData ==null) return context.SetErrorNull<RestEnvironmentJson>( "Runbook is unavailable");
		if (string.IsNullOrEmpty(_bookData.environment)) return context.SetErrorNull<RestEnvironmentJson>( "Environment is missing");
		var fullname = Path.Combine(path, "env-" + _bookData.environment + ".json");
		if (!File.Exists(fullname)) return context.SetErrorNull<RestEnvironmentJson>("Cannot find environment file");
		try
		{
			string json = File.ReadAllText(fullname);			
			var env = JsonConvert.DeserializeObject<RestEnvironmentJson>(json);
			if (env == null) return context.SetErrorNull<RestEnvironmentJson>("Cannot load environment JSON");
			env.id = _bookData.environment;
			return env;
		}
		catch (Exception ex)
		{
			return context.SetErrorNull<RestEnvironmentJson>("Load Environment", ex);
		}
	}
	
	//--------------------------------------------------
	private RestScriptJson? loadScriptFile(ProcessContext context, string path, string fname)
	{
		if (context.HasErrors) return null;
		var fullname = Path.Combine(path, "script-" + fname + ".json");
		if (!File.Exists(fullname)) return context.SetErrorNull<RestScriptJson>($"Cannot find file 'script-{fname}'");
		try
		{
			string json = File.ReadAllText(fullname);
			var script = JsonConvert.DeserializeObject<RestScriptJson>(json);
			if (script == null) return context.SetErrorNull<RestScriptJson>( $"Cannot parse script '{fname}' JSON");
			return script;
		}
		catch (Exception ex)
		{
			return context.SetErrorNull<RestScriptJson>($"Reading script '{fname}'", ex);
		}
	}
	
	//--------------------------------------------------
	private bool loadRunbookScripts(ProcessContext context, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (_bookData ==null) return context.SetError(false, "Runbook is unavailable");
		if (_bookData.scripts==null) return context.SetError(false, "No scripts in the runbook");
		
		_envData = loadEnvironmentFile(context, options.scriptPath);
		foreach (var scriptName in _bookData.scripts)
		{
			var scriptData = loadScriptFile(context, options.scriptPath, scriptName);
			if (scriptData !=null )
			{
				scriptData.name = scriptName; _scripts.Add(scriptData);
			}
		}

		return true;
	}
	
	//--------------------------------------------------
	private bool loadScriptsData(ProcessContext context, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (_bookData ==null) return context.SetError(false, "Runbook is unavailable");
		if (_bookData.scripts==null) return context.SetError(false, "No scripts in the runbook");
		foreach (RestScriptJson script in _scripts)
		{
			reviewTasks(context, script, options);
		}

		return true;
	}
	
	//--------------------------------------------------
	private bool reviewTasks(ProcessContext context, RestScriptJson script, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (script.tasks==null) return context.SetError(false, $"{script.id}: No tasks in script");
		//if (string.IsNullOrEmpty(options.inputPath)) return context.SetError(false, "Data path is not defined");
		foreach (RestTaskJson task in script.tasks)
		{
			if (task.wait != null)
			{
				if (task.wait < 0) return context.SetError(false, $"Task '{script.id}:{task.id}' negative wait time");
				if (task.wait > 60000) return context.SetError(false, $"Task '{script.id}:{task.id}' wait time is too large");
			}
			if (task.content != null && task.body != null)
				context.SetError(false, $"Cannot use body and content in the same task.");
			if (task.body != null) 
				task.hasData = loadDataFile(context, options.inputPath, task.body+".json", "data");
			if (task.assert != null)
			{
				if (task.assert.schema != null)
				{
					switch (task.assert.schema.ToLower())
					{
						case "object":
						case "array":
							task.hasSchema = true;
							break;
						default:
							task.hasSchema = loadDataFile(context, options.schemaPath, "schema-"+task.assert.schema+".json", "schema");
							break;
					}
				}
				if (task.assert.isEmpty()) return context.SetError(false, $"Task '{script.id}:{task.id}' assert is empty");	
			}
			if (task.x509 != null)
			{
				if (string.IsNullOrEmpty(task.x509.file)) return context.SetError(false, $"Task '{script.id}:{task.id}' x509 file is empty");
				task.hasCertificate = loadDataFile(context, options.inputPath, task.x509.file, "certificate");
			}
		}

		return true;
	}
	
	//--------------------------------------------------
	private bool loadDataFile(ProcessContext context, string path, string fname, string ftype)
	{
		if (context.HasErrors) return false;
		var fullname = Path.Combine(path, fname);
		return File.Exists(fullname) || context.SetError(false, $"Cannot find {ftype} file "+fname);
	}
	
	
	#endregion

	#region Validate Scripts

	public RunBook ValidateScripts(ProcessContext context)
	{
		var res = new RunBook();
		if (context.HasErrors) return res;
		if (_bookData==null) return context.SetError(res, "Runbook is missing");
		if (string.IsNullOrEmpty(_bookData.title)) return context.SetError(res, "Runbook title is missing");

		if (_envData==null) return context.SetError(res, "Environment is missing");
		if (string.IsNullOrEmpty(_envData.title)) return context.SetError(res, "Environment title is missing");
		if (_envData.values == null) return context.SetError(res, "Environment values are missing");

		res.title = _bookData.title;
		res.environment = new RestEnvironment(_envData);
		foreach (var script in _scripts)
		{
			validateScript(context, script);
			if (context.HasErrors) break;
			res.scripts.Add(new RestScript(script));
		}
		return res;
	}

	private bool validateScript(ProcessContext context, RestScriptJson script)
	{
		if (context.HasErrors) return false;
		if (string.IsNullOrEmpty(script.id)) return context.SetError(false, $"Runbook script '{script.name}': id is missing");
		if (string.IsNullOrEmpty(script.title)) return context.SetError(false, $"Script '{script.id}': title is missing");
		if (script.tasks == null) return context.SetError(false, $"Script '{script.id}': tasks are missing");
		foreach (var task in script.tasks)
		{
			validateTask(context, script.id, task);
		}
		return true;
	}

	private bool validateTask(ProcessContext context, string sid, RestTaskJson task)
	{
		if (context.HasErrors) return false;
		if (task.disabled) return true;
		if (string.IsNullOrEmpty(task.id)) return context.SetError(false, $"Script '{sid}': task has no ID");
		if (string.IsNullOrEmpty(task.title)) return context.SetError(false, $"Task '{sid}:{task.id}': has no title");
		if (string.IsNullOrEmpty(task.method)) return context.SetError(false, $"Task '{sid}:{task.id}': has no method");
		if (string.IsNullOrEmpty(task.url)) return context.SetError(false, $"Task '{sid}:{task.id}': has no URL");
		
		task.method = task.method.ToUpper();	
		switch (task.method)
		{
			case "HEAD":
			case "GET":
			case "OPTIONS":
			case "DELETE": break;
			case "COPY":
			case "POST":
			case "MERGE":
			case "PATCH":
			case "PUT":
				if (string.IsNullOrEmpty(task.body) && task.content==null) 
					return context.SetError(false, $"Task {sid}/{task.id}: missing body");
				break;
			default: 	return context.SetError(false, $"Task {sid}/{task.id}: unsupported method");
		}
		
		if (task.read != null) {
			foreach (var part in task.read)
			{
				if (string.IsNullOrEmpty(part.locate)) return context.SetError(false, $"Task '{sid}:{task.id}': read.locate is missing");
				if (string.IsNullOrEmpty(part.target)) return context.SetError(false, $"Task '{sid}:{task.id}': read.target is missing");
			}
		}
		if (task.x509 != null)
		{
			if (string.IsNullOrEmpty(task.x509.file)) return context.SetError(false, $"Task '{sid}:{task.id}': x509.file is missing");
		}
		return true;
	}
	
	#endregion

}