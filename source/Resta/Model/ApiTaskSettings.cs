#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;
using System.Threading;

namespace Resta.Model
{
	public class ApiTaskSettings
	{
		public int? timeout;
		public Dictionary<string, string> header;
	}
}