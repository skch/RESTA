#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

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
		public string inputPath = "";
		public string outputPath = "";
		public string schemaPath = "";
		public bool toSaveSuccess = false;
		public bool displayLog = false;
		public bool includeResponseHeader = false;
		public bool failFast = false;
		public bool RemoveRaw = true;
		public bool ToShowVariables = true;
		
		public RestApiCase()
		{
			
		}
		
		
		//===========================================================
		public TestResults Execute(ProcessContext context, RestEnvironment env, RestScript script)
		{
			var empty = new TestResults(false, 0, 0);
			if (context.HasErrors) return empty;
			var scriptTitle = (string.IsNullOrEmpty(script.title)) ? script.id : script.title;

			Console.WriteLine("Script {0} in {1}", scriptTitle, env.title);
			int countFailed = 0;
			int countPassed = 0;
			foreach (var task in script.tasks)
			{
				deleteOldReportFile(context, "api-"+script.id + "-"+task.id+".json");
				var rsp = executeTask(context, env, script, task);
				bool success = reviewResponse(context, env, task, rsp);
				saveReport(context, rsp, "api-"+script.id + "-"+task.id+".json");
				if (!success & failFast)
				{
					verbose("Terminating script because of failure");
					return new TestResults(false, 1, countPassed);
				}
				if (success) countPassed++; else countFailed++;
			}
			return new TestResults(countFailed == 0, countFailed, countPassed);
		}


		//--------------------------------------------------
		private ApiCallReport executeTask(ProcessContext context, RestEnvironment env, RestScript script, RestTask task)
		{
			var res = new ApiCallReport();
			if (context.HasErrors) return res;
			beforeTask(context, task);
			Console.Write("  - {0}:", task.title);
			initiateReport(context, res, env, script, task);
			var client = createRestClient(context, task, res);
			//ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			//if (task.x509 != null) setClientCertificate(context, client, task, res);
			
			var request = prepareRequest(context, res, env, script, task);			
			var response = makeRestCall(context, res, client, request);
			updateReport(context, res, response);
			return res;
		}


		#region Before the call
		
		//--------------------------------------------------
		private bool initiateReport(ProcessContext context, ApiCallReport report, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return false;
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

				report.scriptid = script.id;
				report.title = task.title;
				report.taskid = task.id;
				report.url = task.method+" "+path;
				report.time = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss");
				return true;
			} catch (Exception ex)
			{
				return context.SetError(false, "Prepare Result", ex);
			}
		}
		
		//--------------------------------------------------
		private RestClient? createRestClient(ProcessContext context, RestTask task, ApiCallReport res)
		{
			if (context.HasErrors) return null;
			if (task.basepath == null) return context.SetErrorNull<RestClient>("Base Path is missing");
			try
			{
				var options = new RestClientOptions {
				  BaseUrl = new Uri(task.basepath), 
				  ThrowOnAnyError = false,
					RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
				};
				if (task.x509 != null)
				{
					Console.Write("🔑");
					if (string.IsNullOrEmpty(task.x509?.file)) return context.SetErrorNull<RestClient>("Certificate file is missing");
					var certificate = getClientCertificate(context, task.x509?.file, task.x509?.password);
					if (certificate == null) return null;
					options.ClientCertificates = new X509CertificateCollection() { certificate };
					res.security = task.x509?.file;
				}

				var client = new RestClient(options);
				return client;
			} catch (Exception ex)
			{
				return context.SetErrorNull<RestClient>( "Create Rest Client", ex);
			}
		}
		
		
		//--------------------------------------------------
		private X509Certificate2? getClientCertificate(ProcessContext context, string? cfname, string? pass)
		{
			if (context.HasErrors) return null;
			try
			{
				if (string.IsNullOrEmpty(cfname)) return context.SetErrorNull<X509Certificate2>("Certificate file name is missing");
				var certFile = Path.Combine(inputPath, cfname);
				verbose($"Reading certificate {certFile}");
				if (!File.Exists(certFile)) return context.SetErrorNull<X509Certificate2>("Certificate file does not exist: "+certFile);
				return new X509Certificate2(certFile, pass);
			} catch (Exception ex)
			{
				return context.SetErrorNull<X509Certificate2>("Set Client Certificate", ex);
			}
		}

		//--------------------------------------------------
		private bool setClientCertificate(ProcessContext context, RestClient? client, RestTask task, ApiCallReport res)
		{
			if (context.HasErrors) return false;
			if (client == null) return context.SetError(false, "Rest Client is not initialized");
			try
			{
				Console.Write("🔑");
				if (string.IsNullOrEmpty(task.x509?.file)) return context.SetError(false, "Certificate file is missing");
				var certFile = Path.Combine(inputPath, task.x509.file);
				verbose($"Reading certificate {certFile}");
				if (!File.Exists(certFile)) return context.SetError(false, "Certificate file does not exist: "+certFile);
				X509Certificate2 certificate = new X509Certificate2(certFile, task.x509.password);
				
				var options = new RestClientOptions();
				options.ClientCertificates = new X509CertificateCollection() { certificate };
				//client.Proxy = new WebProxy();		
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
		private RestRequest? prepareRequest(ProcessContext context, ApiCallReport res, RestEnvironment env, RestScript script, RestTask task)
		{
			if (context.HasErrors) return null;
			if (task.urlpath == null) return context.SetErrorNull<RestRequest>("Task Url is missing");
			//Method method = Method.Get;
			try
			{
				var method = (Method)Enum.Parse(typeof(Method), task.method, true);
				// if (!Enum.TryParse(task.method, out RestSharp.Method method, true))
				// {
				// 	res.warnings.Add("Invalid method " + task.method);
				// 	return null;
				// }
				
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

				string jbody = "";
				string ext = ".json";
				
				if (!string.IsNullOrEmpty(task.body))
					(ext,jbody) = readContentFromFile(res, env, request, task.body);
				
				if (task.content!=null) jbody = readContent(res, env, request, task.content);
				if (!string.IsNullOrEmpty(jbody)) res.input = addRequestBody(res, env, request, ext, jbody);
				request.Timeout = 5000;
				if (script.shared?.timeout != null) request.Timeout = (int)script.shared.timeout;
				if (task.timeout != 0) request.Timeout = (int)task.timeout;
				
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.DefaultConnectionLimit = 9999;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
				ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;


				return request;
			}
			catch (Exception ex)
			{
				res.warnings.Add("REST Client: "+ex.Message);
				return null;
			}
			
		}

		//--------------------------------------------------
		private RestResponse? makeRestCall(ProcessContext context, ApiCallReport res, RestClient? client, RestRequest? request)
		{
			if (context.HasErrors) return null;
			try
			{
				if (client == null || request == null) return null;
				var start = DateTime.Now;
				RestResponse? response = client.Execute(request);
				var fd = DateTime.Now - start;
				res.duration = (long)fd.TotalMilliseconds;
				return response;
			} catch (Exception ex)
			{
				return context.SetErrorNull<RestResponse>( "Get HTTP Response", ex);
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
		private void addToHeader(RestEnvironment? env, ApiCallReport? res, Dictionary<string, string> header)
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
		private void setRequestHeader(RestRequest? request, ApiCallReport? res)
		{
			if (res == null) return;
			if (request == null) return;
			if (res.header == null) return;
			if (res.header.Count == 0) return;
			foreach (var key in res.header.Keys)
				request.AddHeader(key, res.header[key]);
		}
		
		//--------------------------------------------------
		private object? addRequestBody(ApiCallReport res,  RestEnvironment? env, RestRequest? request, string ext, string jbody)
		{
			if (request == null) return null;
			try
			{
				string rbody = mustache(jbody, env);
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
				res.warnings.Add("Cannot process request body. "+ex.Message);
				return null;
			}
		}


		//--------------------------------------------------
		private (string, string) readContentFromFile(ApiCallReport res,  RestEnvironment? env, RestRequest? request, string fname)
		{
			var empty = ("", "");
			if (request == null) return empty;
			try
			{
				string fullname = Path.Combine(inputPath, fname);
				verbose($"Reading data {fname}");
				if (!File.Exists(fullname))
					fullname = Path.Combine(inputPath, fname + ".json");
				if (!File.Exists(fullname))
				{
					res.warnings.Add("Cannot find body file: "+fullname);
					return empty;
				}
				string ext = Path.GetExtension(fullname).ToLower();
				string jdata = File.ReadAllText(fullname);
				return (ext, jdata);
			}
			catch (Exception ex)
			{
				res.warnings.Add("Cannot read request body "+fname+". "+ex.Message);
				return empty;
			}
		}

		//--------------------------------------------------
		private string readContent(ApiCallReport res,  RestEnvironment? env, RestRequest? request, dynamic content)
		{
			if (request == null) return "";
			try
			{
				return JsonConvert.SerializeObject(content);
			}
			catch (Exception ex)
			{
				res.warnings.Add("Cannot parse request content. "+ex.Message);
				return "";
			}
		}


		//--------------------------------------------------
		private object? addRequestBody1(ApiCallReport res,  RestEnvironment? env, RestRequest? request, string fname)
		{
			if (request == null) return null;
			try
			{
				string fullname = Path.Combine(inputPath, fname);
				verbose($"Reading data {fname}");
				if (!File.Exists(fullname))
					fullname = Path.Combine(inputPath, fname + ".json");
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
		private bool beforeTask(ProcessContext context, RestTask task)
		{
			if (context.HasErrors) return false;
			if (task.wait == 0) return true;
			Console.Write("  - waiting {0} ms", task.wait);
			Thread.Sleep(task.wait);
			Console.WriteLine();
			return true;
		}
		
		//--------------------------------------------------
		private void updateReport(ProcessContext context, ApiCallReport res, RestResponse? response)
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
		private bool readResponseHeader(ProcessContext context, ApiCallReport res, RestResponse? response)
		{
			if (context.HasErrors) return false;
			if (response == null) return false;
			var rheader = new Dictionary<string, object?>();
			try
			{
				int cnt = 1;
				res.type = "application/json";
				if (response.Headers != null)
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
				if (includeResponseHeader) res.responseHeader = rheader;
				return true;
			} catch (Exception ex)
			{
				res.warnings.Add("Cannot update results: "+ex.Message);
				verbose(ex.StackTrace);
				return false;
			}
		}
		
		//--------------------------------------------------
		private bool readResponseContent(ProcessContext context, ApiCallReport res, RestResponse? response)
		{
			if (context.HasErrors) return false;
			if (response == null) return context.SetError(false, "readResponseContent:Missing response");
			res.htmlcode = (int)response.StatusCode;
			res.raw = response.Content; // raw content as string
			
			if (response.ErrorException != null && res.htmlcode == 0)
			{
				res.raw = JsonConvert.SerializeObject(new WrapException(response.ErrorException));
			}
			return true;
		}
		
		//--------------------------------------------------
		private bool parseResponseContent(ProcessContext context, ApiCallReport res)
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

		#region Review the API call report

		//--------------------------------------------------
		private bool reviewResponse(ProcessContext context, RestEnvironment env, RestTask task, ApiCallReport report)
		{
			if (context.HasErrors) return false;
			try
			{
				if (task.assert != null)
				{
					if (task.assert.response != null)
					{
						var arcode = task.assert.response ?? default(int);
						validateAssert(arcode, report.htmlcode, "Invalid response code", report);
					}
					if (task.assert.responses != null)
					{
						bool match = false;
						foreach (var rcode in task.assert.responses)
						{
							if (report.htmlcode == rcode) { match = true; break; }
						}
						if (!match) report.warnings.Add($"Invalid response code: {report.htmlcode}.");
					}
					if (task.assert.type != null)
						validateAssert(task.assert.type, report.type, "Invalid content type", report);
					if (report.response != null)
					{
						if (RemoveRaw) report.raw = null;

						if (task.assert.schema != null)
						{
							var schema = obtainJsonSchema(context, task.assert.schema);
							validateJsonSchema(context, schema, report);
						}
					}
				}

				readApiResponse(context, env, report, task.read);
				return report.warnings.Count == 0;
			}
			catch (Exception ex)
			{
				return context.SetError(false, "Cannot validate response", ex);
			}
		}
		
		//--------------------------------------------------
		private string obtainJsonSchema(ProcessContext context, string schemaName)
		{
			if (context.HasErrors) return "";
			switch (schemaName.ToLower()) 
			{
				case "object": return "{ \"type\": \"object\" }";
				case "array": return "{ \"type\": \"array\" }";
			}
			string fname = Path.Combine(schemaPath, "schema-"+schemaName + ".json");
			if (!File.Exists(fname)) return context.SetError("", "Schema not found " + schemaName);
			try
			{
				return File.ReadAllText(fname);
			} catch (Exception ex)
			{
				return context.SetError("", "Cannot read schema file", ex);
			}
		}
		
		//--------------------------------------------------
		private void validateAssert<T>(T expect, T actual, string msg, ApiCallReport res) where T : IComparable?
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
		private bool readApiResponse(ProcessContext context, RestEnvironment env, ApiCallReport res, ICollection<ApiRead> readin)
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
					if (ToShowVariables) Console.Write(", {0}={1} ", read.target, element);
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
		private void validateJsonSchema(ProcessContext context, string schemaJson, ApiCallReport res)
		{
			if (context.HasErrors) return;
			if (res.warnings.Count > 0) return;
			try
			{
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

		//--------------------------------------------------
		private bool deleteOldReportFile(ProcessContext context, string fname)
		{
			if (context.HasErrors) return false;
			try
			{
				string fileApiResponse = Path.Combine(outputPath, fname);
				if (File.Exists(fileApiResponse)) File.Delete(fileApiResponse);
				return true;
			}
			catch (Exception ex)
			{
				return context.SetError(false, "Cannot delete file "+fname, ex);
			}

		}
		
		//--------------------------------------------------
		private bool saveReport(ProcessContext context, ApiCallReport result, string fname)
		{
			if (context.HasErrors) return false;
			try
			{
				FluentConsole
					.Text(" ")
					.With(c => result.warnings.Count == 0 ? c.Green : c.Yellow)
					.Line(result.warnings.Count == 0 ? "passed" : "failed");
				if (!toSaveSuccess && result.warnings.Count == 0) return true;
				string fileApiResponse = Path.Combine(outputPath, fname);
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
			if (displayLog) Console.WriteLine("    @"+msg);
		}

		private string ucfirst(string input)
		{
			if (string.IsNullOrEmpty(input)) return input;
			return input[0].ToString().ToUpper() + input.Substring(1).ToLower();
		}

		#endregion
	}
}