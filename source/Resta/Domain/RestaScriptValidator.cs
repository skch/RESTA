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
		if (string.IsNullOrEmpty(options.BookName)) return context.SetError(false, "Runbook name is missing");
		_bookData = loadRunbookFile(context, options.BookName);
		setupEnvironment(context, options);
		
		loadRunbookScripts(context, options);
		return loadScriptsData(context, options);
	}
	
	
	//--------------------------------------------------
	private bool setupEnvironment(ProcessContext context, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (_bookData ==null) return context.SetError(false, "Runbook is unavailable");
		if (!string.IsNullOrEmpty(options.EnvironmentName)) _bookData.environment = options.EnvironmentName;
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
		
		_envData = loadEnvironmentFile(context, options.ScriptPath);
		foreach (var scriptName in _bookData.scripts)
		{
			var scriptData = loadScriptFile(context, options.ScriptPath, scriptName);
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
		
		_envData = loadEnvironmentFile(context, options.ScriptPath);
		foreach (RestScriptJson script in _scripts)
		{
			loadScriptData(context, script, options);
		}

		return true;
	}
	
	//--------------------------------------------------
	private bool loadScriptData(ProcessContext context, RestScriptJson script, RestaParams options)
	{
		if (context.HasErrors) return false;
		if (script.tasks==null) return context.SetError(false, $"{script.id}: No tasks in script");
		if (string.IsNullOrEmpty(options.InputPath)) return context.SetError(false, "Data path is not defined");
		foreach (RestTaskJson task in script.tasks)
		{
			if (task.body != null) task.hasData = loadDataFile(context, options.InputPath, task.body+".json", "data");
			if (task.assert != null)
			{
				if (task.assert.schema != null) 
					task.hasSchema = loadDataFile(context, options.SchemaPath, task.assert.schema+".json", "schema");
				if (task.assert.isEmpty()) return context.SetError(false, $"Task '{script.id}:{task.id}' assert is empty");	
			}
			if (task.x509 != null)
			{
				if (string.IsNullOrEmpty(task.x509.file)) return context.SetError(false, $"Task '{script.id}:{task.id}' x509 file is empty");
				task.hasCertificate = loadDataFile(context, options.InputPath, task.x509.file, "certificate");
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
			ValidateScript(context, script);
			if (context.HasErrors) break;
			res.scripts.Add(new RestScript(script));
		}
		return res;
	}
	
	public bool ValidateScript(ProcessContext context, RestScriptJson script)
	{
		if (context.HasErrors) return false;
		if (string.IsNullOrEmpty(script.id)) return context.SetError(false, $"Runbook script '{script.name}': id is missing");
		if (string.IsNullOrEmpty(script.title)) return context.SetError(false, $"Script '{script.id}': title is missing");
		if (script.tasks == null) return context.SetError(false, $"Script '{script.id}': tasks are missing");
		foreach (var task in script.tasks)
		{
			ValidateTask(context, script.id, task);
		}
		

		
		return true;
	}
	
	public bool ValidateTask(ProcessContext context, string sid, RestTaskJson task)
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
			case "GET":
			case "DELETE": break;
			case "POST":
			case "PUT":
				if (string.IsNullOrEmpty(task.body)) 
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