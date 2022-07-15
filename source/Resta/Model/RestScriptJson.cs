#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model;

public class RestScriptJson
{
	public string? id;
	public string? title;
	public ApiTaskSettingsJson? shared;
	public List<RestTaskJson>? tasks;
	public string name = "";
}