#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using Newtonsoft.Json;

namespace Resta.Model;

[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
public class RestScriptJson
{
	public string? id;
	public string? title;
	public ApiTaskSettingsJson? shared;
	public List<RestTaskJson>? tasks;
	public string name = "";
}