#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using HandlebarsDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Nito.AsyncEx;
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
		public bool DisplayLog = false;
		public bool IncludeResponseHeader = false;
		
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

				task.method = ucfirst(task.method);	
				switch (task.method)
				{
					case "Get":
					case "Delete": break;
					case "Post":
					case "Put":
						if (string.IsNullOrEmpty(task.body)) 
							return context.SetError(false, $"Task {script.id}/{task.id}: missing body");
						break;
					default: 	return context.SetError(false, $"Task {script.id}/{task.id}: unsupported method");
				}
			}
			return true;
			
		}

		private string ucfirst(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
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
			Console.Write("  - {0}:", task.title);
			var res = createResultObject(context, env, script, task);
			var opt = createCallOptions(context, env, script, task);
			
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if (task.x509 != null) setClientCertificate(context, opt, task, res);
			
			var client = createRestClient(context, opt, env, script, task);
			var request = prepareRequest(context, res, env, script, task);
				
			var response = getResponse(context, res, client, request);
			updateResult(context, res, response);
			return res;
		}

		private RestClient createRestClient(ProcessContext context, RestClientOptions opt, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			try
			{
				var hc = new HttpClient();
				hc.BaseAddress = new Uri(task.basepath);
				var client = new RestClient(hc, opt);
				return client;
			} catch (Exception ex)
			{
				return context.SetError<RestClient>(null, "Create Rest Client", ex);
			}
		}

		private bool setClientCertificate(ProcessContext context, RestClientOptions opt, RestTask task, ApiCallResult res)
		{
			if (context.HasErrors) return false;
			try
			{
				Console.Write("🔑");
				if (string.IsNullOrEmpty(task.x509.file)) return context.SetError(false, "Certificate file is missing");
				var certFile = Path.Combine(InputPath, task.x509.file);
				if (!File.Exists(certFile)) return context.SetError(false, "Certificate file does not exist: "+certFile);
				X509Certificate2 certificate = new X509Certificate2(certFile, task.x509.password);
				
				opt.ClientCertificates = new X509CertificateCollection() { certificate };
				opt.Proxy = new WebProxy();		
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;		
				ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
				res.security = task.x509.file;
				return true;
			} catch (Exception ex)
			{
				return context.SetError(false, "Set Client Certificate", ex);
			}
		}

		private ApiCallResult createResultObject(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			try
			{
				string path = mustache(task.url, env);
				verbose($"Task URL {path}");

				var uri = new Uri(path);    
				task.basepath = uri.GetLeftPart(System.UriPartial.Authority);
				task.urlpath = uri.PathAndQuery;
				verbose($"Task base {task.basepath}");
				verbose($"Task path {task.urlpath}");
				var res = new ApiCallResult
				{
					scriptid = script.id,
					taskid = task.id,
					url = task.method+" "+path,
					time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")
				};
				return res;
			} catch (Exception ex)
			{
				return context.SetError<ApiCallResult>(null, "Prepare Result", ex);
			}
		}

		private void verbose(string msg)
		{
			if (DisplayLog) Console.WriteLine("    @"+msg);
		}

		private RestClientOptions createCallOptions(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			try
			{
				var opt = new RestClientOptions();
				return opt;
			} catch (Exception ex)
			{
				return context.SetError<RestClientOptions>(null, "Prepare Client Options", ex);
			}
		}
		

		#region Before the call
		

		//--------------------------------------------------
		private RestRequest prepareRequest(ProcessContext context, ApiCallResult res, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			try
			{
				if (!Enum.TryParse(task.method, out RestSharp.Method method))
				{
					res.warnings.Add("Invalid method " + task.method);
					return null;
				}

				var request = new RestRequest(task.urlpath, method);
				if (script.shared?.header != null) addToHeader(env, res, script.shared.header);
				if (task.header != null) addToHeader(env, res, task.header);
				setRequestHeader(request, res);
				res.input = null;
				if (task.body != null) res.input = addRequestBody(res, env, request, task.body);
				request.Timeout = 5000;
				if (script.shared.timeout != null) request.Timeout = (int)script.shared.timeout;
				if (task.timeout != null) request.Timeout = (int)task.timeout;
				
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.DefaultConnectionLimit = 9999;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

				
				return request;
			}
			catch (Exception ex)
			{
				res.warnings.Add("REST Client: "+ex.Message);
				return null;
			}
			
		}

		//--------------------------------------------------
		private RestResponse getResponse(ProcessContext context, ApiCallResult res, RestClient client, RestRequest request)
		{
			if (context.HasErrors) return null;
			try
			{
				if (client == null || request == null) return null;
				var start = DateTime.Now;
				//var task = client.ExecuteAsync(request);
				
				var response = AsyncContext.Run(() => client.ExecuteAsync(request));
			
				//IRestResponse response = client.Execute(request);
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
			} catch (Exception ex)
			{
				return context.SetError<RestResponse>(null, "Get HTTP Response", ex);
			}
			
			
		}

		//--------------------------------------------------
		private string mustache(string source, RestEnvironment env)
		{
			var template = Handlebars.Compile(source);
			var list = env.includingDynamic();
			return template(list);
		}

		//--------------------------------------------------
		private void addToHeader(RestEnvironment env, ApiCallResult res, Dictionary<string, string> header)
		{
			foreach (var key in header.Keys)
			{
				string value = mustache(header[key], env);
				if (res.header.ContainsKey(key))
				{
					res.header[key] = value;
				} else 
				res.header.Add(key, value);
			}
		}

		//--------------------------------------------------
		private void setRequestHeader(RestRequest request, ApiCallResult res)
		{
			if (res.header.Count == 0) return;
			foreach (var key in res.header.Keys)
				request.AddHeader(key, res.header[key]);
		}

		//--------------------------------------------------
		private object addRequestBody(ApiCallResult res,  RestEnvironment env, RestRequest request, string fname)
		{
			try
			{
				string fullname = Path.Combine(InputPath, fname);
				verbose($"Reading data {fname}");
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
		private void updateResult(ProcessContext context, ApiCallResult res, RestResponse response)
		{
			if (context.HasErrors)
			{
				res.warnings.Add(context.ErrorMessage);
				return;
			}
			if (response == null) {
				res.warnings.Add("Response does not exists");
				return;
			}
			try
			{
				res.htmlcode = (int)response.StatusCode;
				res.raw = response.Content; // raw content as string
				
				var rheader = new Dictionary<string, object>();
				int cnt = 1;
				foreach (var rh in response.Headers)
				{
					if (rheader.ContainsKey(rh.Name))
					{
						rheader.Add($"{rh.Name}-{cnt}", rh.Value);
						cnt++;
					} else
					{
						rheader.Add(rh.Name, rh.Value);
					}
					switch (rh.Name)
					{
						case "Content-Type":
							var parts = rh.Value.ToString().Split(';');
							res.type = parts[0].Trim(); 
							break;
					}
				}

				// TODO: Process different response content types
				if (IncludeResponseHeader) res.responseHeader = rheader;
				res.response = parseAsJson(res.raw);
				if (res.response!=null) res.raw = null;
			}
			catch (Exception ex)
			{
				res.warnings.Add("Cannot update results: "+ex.Message);
				verbose(ex.StackTrace);
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
					var element = locateByPath(context, token, read.locate);
					if (string.IsNullOrEmpty(element)) element = "~";
					env.SetValue(read.target, element);
					Console.Write(", {0}={1} ", read.target, element);
				}
				return true;

			}
			catch (Exception ex)
			{
				return context.SetError(false, "Read API response", ex);
			}
		}

		private string locateByPath(ProcessContext context, JToken token, string query)
		{
			if (context.HasErrors) return "~";
			try
			{
				var element = token.SelectToken(query);
				return (string)element;
			} catch (Exception ex)
			{
				return context.SetError("~~", "Cannot execute query "+query+". "+ex.Message);
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