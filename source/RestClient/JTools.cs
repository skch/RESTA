#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skch.rest
{
	public enum JsonType { JsonNone, JsonEmpty, JsonValue, JsonObject, JsonArray }

	public class JTools
	{
		public static string Attr(JObject data, string name)
		{
			if (data == null) return "";
			JToken val;
			if (data.TryGetValue(name, out val))
			{
				return val.ToString();
			}
			return "";
		}

		public static JsonType GetElementType(JToken jdata)
		{
			if (jdata == null) return JsonType.JsonNone;
			if (jdata is JArray) return JsonType.JsonArray;
			if (jdata is JObject) return JsonType.JsonObject;
			if (jdata is JValue) return JsonType.JsonValue;
			return JsonType.JsonEmpty;
		}


		public static JsonType GetElementType(JToken jdata, string name)
		{
			if (jdata == null) return JsonType.JsonNone;
			if (!(jdata is JObject)) return JsonType.JsonNone;
			var data = jdata as JObject;
			JToken val;
			if (data.TryGetValue(name, out val))
			{
				if (val is JArray) return JsonType.JsonArray;
				if (val is JObject) return JsonType.JsonObject;
				if (val is JValue) return JsonType.JsonValue;
				return JsonType.JsonEmpty;
			}
			return JsonType.JsonNone;

		}


	}
}
