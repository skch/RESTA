using Newtonsoft.Json;
using Resta.Model;

namespace Resta.Domain;

public class ScriptBuilder
{

	private string root = "";
	private string name = "test";
	public bool buildRunbook(ProcessContext context, RestaParams cparams)
	{
		if (context.HasErrors) return false;
		Console.WriteLine("Creating scripts...");
		if (String.IsNullOrEmpty(cparams.bookName)) return context.SetError(false, "Runbook name is missing");
		root = cparams.scriptPath;
		name = Path.GetFileNameWithoutExtension(cparams.bookName);
		createRunbook(context, cparams.bookName);
		createEnvironment(context);
		createSchemas(context);
		createData(context);
		createScript(context);
		createCommand(context);
		if (!context.HasErrors)
		Console.WriteLine($"Runbook {cparams.bookName} generated.");
		return true;
	}
	
	//-------------------------------------
	private bool createRunbook(ProcessContext context, string fname)
	{
		if (context.HasErrors) return false;
		try
		{
			var data = new RunbookJson
			{
				title = "Generated Runbook",
				environment = "main",
				scripts = new string[] { "crud" }
			};
			var jtext = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(fname, jtext); 
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createRunbook: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private bool createEnvironment(ProcessContext context)
	{
		if (context.HasErrors) return false;
		try
		{
			var data = new RestEnvironmentJson()
			{
				id="main",
				title = "Primary Environment",
				values = new Dictionary<string, string>()
			};
			data.values.Add("base","https://myserver.com/v1");
			string fname = Path.Combine(root, "env-main.json");
			var jtext = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(fname, jtext); 
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createEnvironment: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private bool createSchemas(ProcessContext context)
	{
		if (context.HasErrors) return false;
		try
		{
			string spath = Path.Combine(root, "schema");
			if (!Directory.Exists(spath)) Directory.CreateDirectory(spath);
			string fname = Path.Combine(spath, "schema-object.json");
			var data = new SchemaJson { description = "Any Object", type = "object" };
			var jtext = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(fname, jtext);
			fname = Path.Combine(spath, "schema-array.json");
			data = new SchemaJson { description = "Any Array", type = "array" };
			jtext = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(fname, jtext);
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createSchemas: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private bool createData(ProcessContext context)
	{
		if (context.HasErrors) return false;
		try
		{
			string spath = Path.Combine(root, "data");
			if (!Directory.Exists(spath)) Directory.CreateDirectory(spath);
			string fname = Path.Combine(spath, "new-resource.json");
			File.WriteAllText(fname, "{ }");
			fname = Path.Combine(spath, "update.json");
			File.WriteAllText(fname, "{ }");
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createData: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private bool createScript(ProcessContext context)
	{
		if (context.HasErrors) return false;
		var data = createNewScript(context);
		generatePostTask(context, data);
		generateGetTask(context, data);
		generatePutTask(context, data);
		generateDeleteTask(context, data);
		saveScript(context, data);
		try
		{
			string fname = Path.Combine(root, "script-crud.json");
			var jtext = JsonConvert.SerializeObject(data, Formatting.Indented);
			File.WriteAllText(fname, jtext); 
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createScript: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private RestScriptJson? createNewScript(ProcessContext context)
	{
		if (context.HasErrors) return null;
		try
		{
			var data = new RestScriptJson()
			{
				id="crud",
				title = "CRUD script example", 
				shared = new ApiTaskSettingsJson
				{
					timeout = 2000,
					header = new Dictionary<string, string>()
				},
				tasks = new List<RestTaskJson>()
			};
			data.shared.header.Add("Accept","application/json");
			data.shared.header.Add("Content-Type","application/json");
			return data;
		} catch (Exception ex)
		{
			return context.SetErrorNull<RestScriptJson>( "ERROR in createNewScript: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private RestTaskJson? generatePostTask(ProcessContext context, RestScriptJson? script)
	{
		if (context.HasErrors) return null;
		if (script==null) return context.SetErrorNull<RestTaskJson>( "Script is missing");
		if (script.tasks==null) return context.SetErrorNull<RestTaskJson>( "Script tasks are missing");
		try
		{
			var data = new RestTaskJson()
			{
				id="01",
				title = "01: Create Resource", 
				description = "Create new resource on the server", 
				disabled = false, 
				method = "POST", 
				body = "new-resource", 
				read = new []
				{
					new ApiReadJson {  locate = "response.id", target = "rid" }					
				},
				url = "{{base}}/resource", 
				assert = new ApiAssert { response = 201, type = "application/json", schema = "object" }
			};
			script.tasks.Add(data);
			return data;
		} catch (Exception ex)
		{
			return context.SetErrorNull<RestTaskJson>( "ERROR in generateDeleteTask: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private RestTaskJson? generateGetTask(ProcessContext context, RestScriptJson? script)
	{
		if (context.HasErrors) return null;
		if (script==null) return context.SetErrorNull<RestTaskJson>( "Script is missing");
		if (script.tasks==null) return context.SetErrorNull<RestTaskJson>( "Script tasks are missing");
		try
		{
			var data = new RestTaskJson()
			{
				id="02",
				title = "02: Read Resource", 
				description = "Read the resource created in step 01", 
				disabled = false, 
				method = "GET", 
				url = "{{base}}/resource/{{rid}}", 
				assert = new ApiAssert { response = 200, type = "application/json", schema = "object" }
			};
			script.tasks.Add(data);
			return data;
		} catch (Exception ex)
		{
			return context.SetErrorNull<RestTaskJson>( "ERROR in generateDeleteTask: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private RestTaskJson? generatePutTask(ProcessContext context, RestScriptJson? script)
	{
		if (context.HasErrors) return null;
		if (script==null) return context.SetErrorNull<RestTaskJson>( "Script is missing");
		if (script.tasks==null) return context.SetErrorNull<RestTaskJson>( "Script tasks are missing");
		try
		{
			var data = new RestTaskJson()
			{
				id="03",
				title = "03: Update Resource", 
				description = "Update the resource created in step 01", 
				disabled = false, 
				method = "PUT", 
				body = "update", 
				url = "{{base}}/resource/{{rid}}", 
				assert = new ApiAssert { response = 200, type = "application/json", schema = "object" }
			};
			script.tasks.Add(data);
			return data;
		} catch (Exception ex)
		{
			return context.SetErrorNull<RestTaskJson>( "ERROR in generateDeleteTask: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private RestTaskJson? generateDeleteTask(ProcessContext context, RestScriptJson? script)
	{
		if (context.HasErrors) return null;
		if (script==null) return context.SetErrorNull<RestTaskJson>( "Script is missing");
		if (script.tasks==null) return context.SetErrorNull<RestTaskJson>( "Script tasks are missing");
		try
		{
			var data = new RestTaskJson()
			{
				id="04",
				title = "04: Delete Resource", 
				description = "Delete the resource created in step 01", 
				disabled = false, 
				method = "DELETE", 
				url = "{{base}}/resource/{{rid}}", 
				assert = new ApiAssert { response = 200 }
			};
			script.tasks.Add(data);
			return data;
		} catch (Exception ex)
		{
			return context.SetErrorNull<RestTaskJson>( "ERROR in generateDeleteTask: " + ex.Message);
		}
	}
	
	//-------------------------------------
	private bool saveScript(ProcessContext context, RestScriptJson? script)
	{
		if (context.HasErrors) return false;
		try
		{
			string fname = Path.Combine(root, "script-crud.json");
			var jtext = JsonConvert.SerializeObject(script, Formatting.Indented);
			File.WriteAllText(fname, jtext); 
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in saveScript: " + ex.Message);
		}
	}

	//-------------------------------------
	private bool createCommand(ProcessContext context)
	{
		if (context.HasErrors) return false;
		try
		{
			string fname = Path.Combine(root, $"run-{name}.sh");
			string code = "# Run RESTA test\nmkdir result\nresta demo.json -sc:schema -in:data -out:result -keep";
			File.WriteAllText(fname, code); 
			return true;
		} catch (Exception ex)
		{
			return context.SetError(false, "ERROR in createCommand: " + ex.Message);
		}
	}
}