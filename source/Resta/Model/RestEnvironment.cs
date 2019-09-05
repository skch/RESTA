#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;

namespace Resta.Model
{
	public class RestEnvironment
	{
		public string title;
		public Dictionary<string, string> values = new Dictionary<string, string>();
	}
}