#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model
{
	public class RunBook
	{
		public string title = "";
		public RestEnvironment environment = new RestEnvironment();
		public List<RestScript> scripts = new List<RestScript>();
	}
}