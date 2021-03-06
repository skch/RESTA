#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;

namespace Resta.Model
{
	public class RestTask
	{
		public string id;
		public bool disabled = false;
		public string title;
		public string description;
		public string method;
		public string url;
		public string body;
		public Dictionary<string, string> header;
		public ApiAssert assert;
		public ApiRead[] read;
	}
}