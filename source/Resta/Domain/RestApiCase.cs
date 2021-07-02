#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Resta.Model;
using RestSharp;

namespace Resta.Domain
{
	public class RestApiCase
	{

		public string InputPath = "";
		public string OutputPath = "";
		public string SchemaPath = "";
		public bool ToSaveSuccess = false;
		
		public RestApiCase()
		{
			
		}
		
		//===========================================================
		public bool Validate(ProcessContext context, RestEnvironment env, RestScript script)
		{
			if (context.HasErrors) return false;

			foreach (var task in script.tasks)
			{
				if (task.disabled) continue;
				if (string.IsNullOrEmpty(task.url)) 
					return context.SetError(false, $"Task {script.id}/{task.id}: missing url");
				if (task.assert == null) 
					return context.SetError(false, $"Task {script.id}/{task.id}: missing assert");
				switch (task.method)
				{
					case "GET":
					case "DELETE": break;
					case "POST":
					case "PUT":
						if (string.IsNullOrEmpty(task.body)) 
							return context.SetError(false, $"Task {script.id}/{task.id}: missing body");
						break;
				}
			}
			return true;
			
		}
		
		//===========================================================
		public bool Execute(ProcessContext context, RestEnvironment env, RestScript script)
		{
			if (context.HasErrors) return false;

			Console.WriteLine("Script {0} in {1}", script.title, env.title);
			foreach (var task in script.tasks)
			{
				ClearResult(context, "api-"+script.id + "-"+task.id+".json");
				var rsp = executeTask(context, env, script, task);
				if (rsp == null) continue;
				ValidateResponse(context, env, task, rsp);
				SaveResponse(context, rsp, "api-"+script.id + "-"+task.id+".json");
			}
			return true;
			
		}

		//--------------------------------------------------
		private ApiCallResult executeTask(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			if (task.disabled) return null;

			string path = mustache(task.url, env);
			var res = new ApiCallResult
			{
				scriptid = script.id,
				taskid = task.id,
				url = task.method+" "+path,
				time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")
			};

			Console.Write("  - {0}:", task.title);
			var client = new RestClient(path) {Timeout = 80000 };
			var request = prepareRequest(res, env, script, task);
			
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			
			var response = getResponse(res, client, request);
			updateResult(res, response);
			return res;
		}

		#region Before the call
		

		//--------------------------------------------------
		private RestRequest prepareRequest(ApiCallResult res, RestEnvironment env, RestScript script, RestTask task)
		{
			try
			{
				if (!Enum.TryParse(task.method, out RestSharp.Method method))
				{
					res.warnings.Add("Invalid method " + task.method);
					return null;
				}
				var request = new RestRequest(method);
				if (script.shared?.header != null) addRequestHeader(env, request, script.shared.header);
				if (task.header != null) addRequestHeader(env, request, task.header);
				res.input = null;
				if (task.body != null) res.input = addRequestBody(res, env, request, task.body);
				
				return request;
			}
			catch (Exception ex)
			{
				res.warnings.Add("REST Client: "+ex.Message);
				return null;
			}
			
			//request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
			//request.AddUrlSegment("id", "123"); // replaces matching token in request.Resource
			
			// add files to upload (works with compatible verbs)
			//request.AddFile(path);
		}

		//--------------------------------------------------
		private IRestResponse getResponse(ApiCallResult res, RestClient client, RestRequest request)
		{
			if (client == null || request == null) return null;
			var start = DateTime.Now;
			IRestResponse response = client.Execute(request);
			var fd = DateTime.Now - start;
			res.duration = (long)fd.TotalMilliseconds;
			if (response.ErrorException != null)
			{
				res.warnings.Add(response.ErrorException.Message);
			}
			return response;
			//client.ExecuteAsync(request, response1 => {
			//	Console.WriteLine(response1.Content);
			//});
		}

		//--------------------------------------------------
		private string mustache(string source, RestEnvironment env)
		{

			var template = Handlebars.Compile(source);
			return template(env.values);
		}

		//--------------------------------------------------
		private void addRequestHeader(RestEnvironment env, RestRequest request, Dictionary<string, string> header)
		{
			foreach (var key in header.Keys)
			{
				request.AddHeader(key, mustache(header[key], env));
			}
		}

		//--------------------------------------------------
		private object addRequestBody(ApiCallResult res,  RestEnvironment env, RestRequest request, string fname)
		{
			try
			{
				string fullname = Path.Combine(InputPath, fname);
				if (!File.Exists(fullname))
					fullname = Path.Combine(InputPath, fname + ".json");
				if (!File.Exists(fullname))
				{
					res.warnings.Add("Cannot find body file: "+fullname);
					return null;
				}
				string json = mustache(File.ReadAllText(fullname), env);
				string ext = Path.GetExtension(fullname).ToLower();
				switch (ext)
				{
					case ".json":
						var data = JsonConvert.DeserializeObject(json);
						request.RequestFormat = DataFormat.Json;
						request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
						return data;
						
					case ".xml":
						request.RequestFormat = DataFormat.Xml;
						request.AddParameter("application/xml; charset=utf-8", json, ParameterType.RequestBody);
						return new XmlInput(json);
						
					default:
						res.warnings.Add("Unsupported type: "+ext);
						return null;
				}
				
				//request.AddJsonBody(data);
			}
			catch (Exception ex)
			{
				res.warnings.Add("Cannot read request body "+fname+". "+ex.Message);
				return null;
			}
		}

		#endregion

		#region After the call
		
		//--------------------------------------------------
		private void updateResult(ApiCallResult res, IRestResponse response)
		{
			if (response == null) return;
			try
			{
				res.htmlcode = (int)response.StatusCode;
				res.raw = response.Content; // raw content as string
				
				var rheader = new Dictionary<string, object>();
				foreach (var rh in response.Headers)
				{
					rheader.Add(rh.Name, rh.Value);
					switch (rh.Name)
					{
						case "Content-Type":
							var parts = rh.Value.ToString().Split(';');
							res.type = parts[0].Trim(); 
							break;
					}
				}

				res.response = parseAsJson(res.raw);
				if (res.response!=null) res.raw = null;
			}
			catch (Exception ex)
			{
				res.warnings.Add("Exception: "+ex.Message);
			}

		}

		//--------------------------------------------------
		private object parseAsJson(string data)
		{
			try
			{
				return JsonConvert.DeserializeObject(data);
			}
			catch 
			{
				return null;
			}
		}

		#endregion

		#region Validate API call results

		//===========================================================
		public bool ValidateResponse(ProcessContext context, RestEnvironment env, RestTask task, ApiCallResult result)
		{
			if (context.HasErrors) return false;
			if (result == null) return context.SetError(false, "API result is NULL " + task.id);
			try
			{
				if (task.assert != null)
				{
					if (task.assert.response != null)
					{
						int arcode = task.assert.response ?? default(int);
						validateAssert(arcode, result.htmlcode, "Invalid response code", result);
					}
					if (task.assert.responses != null)
					{
						bool match = false;
						foreach (var rcode in task.assert.responses)
						{
							if (result.htmlcode == rcode) { match = true; break; }
						}
						if (!match) result.warnings.Add($"Invalid response code: {result.htmlcode}.");
					}
					if (task.assert.type != null)
						validateAssert(task.assert.type, result.type, "Invalid content type", result);
					if (result.response != null)
					{
						result.raw = null;

						if (task.assert.schema != null)
						{
							string fname = Path.Combine(SchemaPath, "schema-"+task.assert.schema + ".json");
							if (!File.Exists(fname)) return context.SetError(false, "Schema not found " + task.assert.schema);
							validateJsonSchema(fname, result);
						}
					}
				}

				if (task.read != null && result.warnings.Count == 0)
					readApiResponse(context, env, result, task.read);

				return true;

			}
			catch (Exception ex)
			{
				return context.SetError(false, "Cannot validate response", ex);
			}
		}
		
		//--------------------------------------------------
		private void validateAssert<T>(T expect, T actual, string msg, ApiCallResult res) where T : IComparable
		{
			if (res.warnings.Count > 0) return;
			if (expect == null) return;
			

			if (expect.CompareTo(actual) != 0) 
				res.warnings.Add($"{msg}: {actual}. Expected {expect}");
		}

		
		//--------------------------------------------------
		private bool readApiResponse(ProcessContext context, RestEnvironment env, ApiCallResult res, IEnumerable<ApiRead> readin)
		{
			if (context.HasErrors) return false;
			try
			{
				var token = (JToken) res.response;
				foreach (var read in readin)
				{
					var element = (string)token.SelectToken(read.locate);
					if (string.IsNullOrEmpty(element)) element = "~";
					if (env.values.ContainsKey(read.target)) 
						env.values[read.target] = element; 
					else 
						env.values.Add(read.target, element);
					Console.Write(", {0}={1} ", read.target, element);
				}
				return true;

			}
			catch (Exception ex)
			{
				return context.SetError(false, "Read API response", ex);
			}
		}

		//--------------------------------------------------
		private void validateJsonSchema(string fschema, ApiCallResult res)
		{
			if (res.warnings.Count > 0) return;
			try
			{
				string schemaJson = File.ReadAllText(fschema);
				JSchema schema = JSchema.Parse(schemaJson);

				JToken response = (JToken) res.response;
				IList<string> messages;

				if (!response.IsValid(schema, out messages))
				{
					foreach (var msg in messages)
					{
						res.warnings.Add($"Schema: {msg}");
					}
				}
			}
			catch (Exception ex)
			{
				res.warnings.Add($"Schema fatal: {ex.Message}");
			}
				
		}

		//===========================================================
		public bool ClearResult(ProcessContext context, string fname)
		{
			if (context.HasErrors) return false;
			try
			{
				string fileApiResponse = Path.Combine(OutputPath, fname);
				if (File.Exists(fileApiResponse)) File.Delete(fileApiResponse);
				return true;
			}
			catch (Exception ex)
			{
				return context.SetError(false, "Cannot delete file", ex);
			}

		}
		
		//===========================================================
		public bool SaveResponse(ProcessContext context, ApiCallResult result, string fname)
		{
			if (context.HasErrors) return false;
			try
			{
				FluentConsole
					.Text(" ")
					.With(c => result.warnings.Count == 0 ? c.Green : c.Yellow)
					.Line(result.warnings.Count == 0 ? "passed" : "failed");
				//if (result.errors.Count == 0) Console.WriteLine(" completed");
				//else Console.WriteLine(" failed");
				if (!ToSaveSuccess && result.warnings.Count == 0) return true;
				string fileApiResponse = Path.Combine(OutputPath, fname);
				string jsonApiResponse = JsonConvert.SerializeObject(result, Formatting.Indented);
				File.WriteAllText(fileApiResponse, jsonApiResponse);
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(" crashed");
				return context.SetError(false, "Cannot save result", ex);
			}

		}

		

		#endregion
		
	}
}