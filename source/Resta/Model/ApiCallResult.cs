#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;

namespace Resta.Model
{
	public class ApiCallResult
	{
		public string scriptid;
		public string taskid;
		public string url;
		public string time;
		public Dictionary<string, string> header = new Dictionary<string, string>();
		public object input;
		public string security;
		public long duration;
		public int htmlcode;
		public string type;
		public List<string> warnings = new List<string>();
		public object response;
		public string raw;
	}
}