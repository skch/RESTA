#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Resta.Model
{
	public class ApiCallResult
	{
		public string scriptid = "";
		public string taskid = "";
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string? title;
		
		public string url = "";
		public string time = "";
		public Dictionary<string, string>? header = new Dictionary<string, string>();
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public object? input;
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string? security;
		
		public long duration;
		public int htmlcode;
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string? type;
		
		public List<string?> warnings = new List<string?>();
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public object? response;
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public Dictionary<string, object?>? responseHeader;
		
		[JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
		public string? raw;

	}
}