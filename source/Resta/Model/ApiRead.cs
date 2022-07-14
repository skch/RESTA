#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model
{
	public class ApiRead
	{
		public string locate;
		public string target;

		public ApiRead(ApiReadJson data)
		{
			locate = data.locate ?? string.Empty;
			target = data.target ?? string.Empty;
		}
	}
}