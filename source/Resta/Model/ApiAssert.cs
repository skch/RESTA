#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using Newtonsoft.Json;

namespace Resta.Model
{

	[JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
	public class ApiAssert
	{
		public int? response;
		public int[]? responses;
		public string? type;
		public string? schema;

		public bool isEmpty()
		{
			if (response != null) return false;
			if (responses != null) return false;
			if (type != null) return false;
			if (schema != null) return false;
			return true;
		}
	}
}