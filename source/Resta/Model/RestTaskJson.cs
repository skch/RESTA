#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using Newtonsoft.Json;

namespace Resta.Model;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class RestTaskJson
{
	public string? id;
	public bool disabled = false;
	public string? title;
	public string? description;
	public int? timeout;
	public string? method;
	public CertificateSettingsJson? x509; 
	public string? url;
	public string? body;
	public dynamic? content;
	public Dictionary<string, string>? header;
	public ApiAssert? assert;
	public ApiReadJson[]? read;
	public int? wait;
	
	internal bool hasData;
	internal bool hasSchema;
	internal bool hasCertificate;
}