#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model;

public class ApiTaskSettingsJson
{
	public int? timeout;
	public Dictionary<string, string>? header;
}