#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model
{
	public class RestTask
	{
		public string id; 
		public string title;
		public string description = "";
		public int timeout;
		public string method;
		public CertificateSettings? x509;
		public string url;
		public string body;
		public Dictionary<string, string> header = new Dictionary<string, string>();
		public ApiAssert? assert;
		public List<ApiRead> read = new List<ApiRead>();
		public int wait = 0;

		internal string scheme = "";//****
		internal string? basepath;//****
		internal string? urlpath;//****

		public RestTask(RestTaskJson data)
		{
			id = data.id ?? string.Empty;
			title = data.title ?? string.Empty;
			description = data.description ?? string.Empty;
			url = data.url ?? string.Empty;
			body = data.body ?? string.Empty;
			method = data.method ?? "GET";
			wait = data.wait ?? 0;
			if (data.timeout != null) timeout = (int)data.timeout;
			if (data.assert != null) assert = data.assert;
			if (data.read != null)
			{
				foreach (var part in data.read)
				{
					read.Add(new ApiRead(part));
				}
			}
			if (data.header != null) header = data.header;
			if (data.x509 != null)
			{
				x509 = new CertificateSettings
				{
					file = data.x509.file ?? string.Empty,
					password = data.x509.password ?? string.Empty
				};
			}
	
		}
	}
}