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
using System.Security.Cryptography.X509Certificates;
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
		public bool DisplayLog = false;
		public bool IncludeResponseHeader = false;
		public bool FailFast = false;
		public bool RemoveRaw = true;
		
		public RestApiCase()
		{
			
		}
		
		
		//===========================================================
		public bool Execute(ProcessContext context, RestEnvironment env, RestScript script)
		{
			if (context.HasErrors) return false;
			var scriptTitle = (string.IsNullOrEmpty(script.title)) ? script.id : script.title;

			Console.WriteLine("Script {0} in {1}", scriptTitle, env.title);
			bool bigsuccess = true;
			foreach (var task in script.tasks)
			{
				ClearResult(context, "api-"+script.id + "-"+task.id+".json");
				var rsp = executeTask(context, env, script, task);
				bool success = ValidateResponse(context, env, task, rsp);
				SaveResponse(context, rsp, "api-"+script.id + "-"+task.id+".json");
				if (!success & FailFast)
				{
					verbose("Terminating script because of failure");
					return false;
				}
				if (!success) bigsuccess = false;
			}
			return bigsuccess;
		}


		//--------------------------------------------------
		private ApiCallResult? executeTask(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			Console.Write("  - {0}:", task.title);
			var res = createResultObject(context, env, script, task);
			
			var client = createRestClient(context, task);
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			if (task.x509 != null) setClientCertificate(context, client, task, res);
			
			var request = prepareRequest(context, res, env, script, task);			
			var response = getResponse(context, res, client, request);
			updateResult(context, res, response);
			return res;
		}


		#region Before the call
		
		//--------------------------------------------------
		private RestClient? createRestClient(ProcessContext context, RestTask task)
		{
			if (context.HasErrors) return null;
			if (task.basepath == null) return context.SetErrorNull<RestClient>("Base Path is missing");
			try
			{
				var client = new RestClient(task.basepath);
				return client;
			} catch (Exception ex)
			{
				return context.SetErrorNull<RestClient>( "Create Rest Client", ex);
			}
		}

		//--------------------------------------------------
		private bool setClientCertificate(ProcessContext context, RestClient? client, RestTask task, ApiCallResult res)
		{
			if (context.HasErrors) return false;
			if (client == null) return context.SetError(false, "Rest Client is not initialized");
			try
			{
				Console.Write("🔑");
				if (string.IsNullOrEmpty(task.x509?.file)) return context.SetError(false, "Certificate file is missing");
				var certFile = Path.Combine(InputPath, task.x509.file);
				verbose($"Reading certificate {certFile}");
				if (!File.Exists(certFile)) return context.SetError(false, "Certificate file does not exist: "+certFile);
				X509Certificate2 certificate = new X509Certificate2(certFile, task.x509.password);
				
				client.ClientCertificates = new X509CertificateCollection() { certificate };
				client.Proxy = new WebProxy();		
				//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;		
				ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;
				res.security = task.x509.file;
				return true;
			} catch (Exception ex)
			{
				return context.SetError(false, "Set Client Certificate", ex);
			}
		}

		//--------------------------------------------------
		private ApiCallResult createResultObject(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			var res = new ApiCallResult();
			if (context.HasErrors) return res;
			try
			{
				string path = mustache(task.url, env);
				verbose($"Task URL {path}");

				var uri = new Uri(path);   
				task.scheme = uri.GetLeftPart(System.UriPartial.Scheme);
				task.basepath = uri.GetLeftPart(System.UriPartial.Authority);
				task.urlpath = uri.PathAndQuery;
				verbose($"Task base {task.basepath}");
				verbose($"Task path {task.urlpath}");
				return new ApiCallResult
				{
					scriptid = script.id,
					title = script.title,
					taskid = task.id,
					url = task.method+" "+path,
					time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss")
				};
			} catch (Exception ex)
			{
				return context.SetError<ApiCallResult>(res, "Prepare Result", ex);
			}
		}
		
		//--------------------------------------------------
		private RestRequest? prepareRequest(ProcessContext context, ApiCallResult res, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			if (task.urlpath == null) return context.SetErrorNull<RestRequest>("Task Url is missing");
			try
			{
				if (!Enum.TryParse(task.method, out RestSharp.Method method))
				{
					res.warnings.Add("Invalid method " + task.method);
					return null;
				}
				
				if (task.scheme != "http://" && task.scheme != "https://")
				{
					res.warnings.Add("Unsupported protocol: " + res.url);
					return null;
				}


				var request = new RestRequest(task.urlpath, method);
				if (script.shared?.header != null) addToHeader(env, res, script.shared.header);
				addToHeader(env, res, task.header);
				setRequestHeader(request, res);
				res.input = null;
				if (!string.IsNullOrEmpty(task.body)) res.input = addRequestBody(res, env, request, task.body);
				request.Timeout = 5000;
				if (script.shared?.timeout != null) request.Timeout = (int)script.shared.timeout;
				if (task.timeout != 0) request.Timeout = (int)task.timeout;
				
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
		private IRestResponse? getResponse(ProcessContext context, ApiCallResult res, RestClient? client, RestRequest? request)
		{
			if (context.HasErrors) return null;
			try
			{
				if (client == null || request == null) return null;
				var start = DateTime.Now;
				IRestResponse? response = client.Execute(request);
				var fd = DateTime.Now - start;
				res.duration = (long)fd.TotalMilliseconds;
				if (response.ErrorException != null)
				{
					res.warnings.Add(response.ErrorException.Message);
				}
				return response;
			} catch (Exception ex)
			{
				return context.SetErrorNull<IRestResponse>( "Get HTTP Response", ex);
			}
			
			
		}

		//--------------------------------------------------
		private string mustache(string source, RestEnvironment? env)
		{
			if (env == null) return "";
			var template = Handlebars.Compile(source);
			var list = env.includingDynamic();
			return template(list);
		}

		//--------------------------------------------------
		private void addToHeader(RestEnvironment? env, ApiCallResult? res, Dictionary<string, string> header)
		{
			if (res == null) return;
			if (res.header == null) return;
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
		private void setRequestHeader(RestRequest? request, ApiCallResult? res)
		{
			if (res == null) return;
			if (request == null) return;
			if (res.header == null) return;
			if (res.header.Count == 0) return;
			foreach (var key in res.header.Keys)
				request.AddHeader(key, res.header[key]);
		}

		//--------------------------------------------------
		private object? addRequestBody(ApiCallResult res,  RestEnvironment? env, RestRequest? request, string fname)
		{
			if (request == null) return null;
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
				string rbody = mustache(File.ReadAllText(fullname), env);
				string ext = Path.GetExtension(fullname).ToLower();
				switch (ext)
				{
					case ".json":
						var data = JsonConvert.DeserializeObject(rbody);
						request.RequestFormat = DataFormat.Json;
						request.AddJsonBody(rbody);
						return data;
						
					case ".xml":
						request.RequestFormat = DataFormat.Xml;
						request.AddXmlBody(rbody);
						return new XmlInput(rbody);
						
					default:
						res.warnings.Add("Unsupported type: "+ext);
						return null;
				}

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
		private void updateResult(ProcessContext context, ApiCallResult res, IRestResponse? response)
		{
			if (context.HasErrors)
			{
				res.warnings.Add(context.ErrorMessage);
				return;
			}
			if (response == null) {
				res.warnings.Add("Could not make the HTTP call");
				return;
			}

			readResponseHeader(context, res, response);
			readResponseContent(context, res, response);
			parseResponseContent(context, res);
		}
		
		//--------------------------------------------------
		private bool readResponseHeader(ProcessContext context, ApiCallResult res, IRestResponse? response)
		{
			if (context.HasErrors) return false;
			if (response == null) return false;
			var rheader = new Dictionary<string, object?>();
			try
			{
				int cnt = 1;
				foreach (var rh in response.Headers)
				{
					if (rh.Name == null) continue;
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
							if (rh.Value != null)
							{
								var parts = rh.Value.ToString()?.Split(';');
								res.type = parts?[0].Trim(); 
							}
							break;
					}
				}
				if (IncludeResponseHeader) res.responseHeader = rheader;
				return true;
			} catch (Exception ex)
			{
				res.warnings.Add("Cannot update results: "+ex.Message);
				verbose(ex.StackTrace);
				return false;
			}
		}
		
		//--------------------------------------------------
		private bool readResponseContent(ProcessContext context, ApiCallResult res, IRestResponse? response)
		{
			if (context.HasErrors) return false;
			if (response == null) return context.SetError(false, "readResponseContent:Missing response");
			res.htmlcode = (int)response.StatusCode;
			res.raw = response.Content; // raw content as string
			return true;
		}
		
		//--------------------------------------------------
		private bool parseResponseContent(ProcessContext context, ApiCallResult res)
		{
			if (context.HasErrors) return false;
			if (res.type != "application/json") return true;
			try
			{
				res.response = (res.raw == null) ? "": JsonConvert.DeserializeObject(res.raw);
			}
			catch (Exception ex)
			{
				res.warnings.Add("JSON: "+ex.Message);
			}
			if (res.response!=null && RemoveRaw) res.raw = null;
			return true;
		}


		#endregion

		#region Validate API call results

		//--------------------------------------------------
		private bool ValidateResponse(ProcessContext context, RestEnvironment? env, RestTask task, ApiCallResult? result)
		{
			if (context.HasErrors) return false;
			if (result == null) return context.SetError(false, "Api Call Result is missing");
			if (env == null) return context.SetError(false, "Environment is missing");
			try
			{
				if (task.assert != null)
				{
					if (task.assert.response != null)
					{
						var arcode = task.assert.response ?? default(int);
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
						if (RemoveRaw) result.raw = null;

						if (task.assert.schema != null)
						{
							string fname = Path.Combine(SchemaPath, "schema-"+task.assert.schema + ".json");
							if (!File.Exists(fname)) return context.SetError(false, "Schema not found " + task.assert.schema);
							validateJsonSchema(fname, result);
						}
					}
				}

				readApiResponse(context, env, result, task.read);
				return result.warnings.Count == 0;
			}
			catch (Exception ex)
			{
				return context.SetError(false, "Cannot validate response", ex);
			}
		}
		
		//--------------------------------------------------
		private void validateAssert<T>(T expect, T actual, string msg, ApiCallResult res) where T : IComparable?
		{
			if (res.warnings.Count > 0) return;
			if (expect == null)
			{
				res.warnings.Add($"{msg}: {actual}. Expected is missing");
				return;
			}
			if (expect.CompareTo(actual) != 0) 
				res.warnings.Add($"{msg}: {actual}. Expected {expect}");
		}

		
		//--------------------------------------------------
		private bool readApiResponse(ProcessContext context, RestEnvironment env, ApiCallResult res, ICollection<ApiRead> readin)
		{
			if (context.HasErrors) return false;
			if (res.warnings.Count != 0) return true;
			if (res.type != "application/json") return true;
			if (readin.Count == 0) return true;
			try
			{
				if (res.response == null) return false;
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
				if (element == null) return "~";
				var res = (string?)element;
				return res ?? "~";
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

				var response = (JToken?) res.response;
				if (response == null)
				{
					res.warnings.Add($"Cannot parse response");
					return;
				}

				if (!response.IsValid(schema, out IList<string> messages))
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
		public bool SaveResponse(ProcessContext context, ApiCallResult? result, string fname)
		{
			if (context.HasErrors) return false;
			if (result == null) return context.SetError(false, "API Result is not initialized");
			try
			{
				FluentConsole
					.Text(" ")
					.With(c => result.warnings.Count == 0 ? c.Green : c.Yellow)
					.Line(result.warnings.Count == 0 ? "passed" : "failed");
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
		
		#region Utilities
		private void verbose(string? msg)
		{
			if (DisplayLog) Console.WriteLine("    @"+msg);
		}

		private string ucfirst(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
		}

		#endregion
	}
}