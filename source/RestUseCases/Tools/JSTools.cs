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

namespace RestUseCases.Tools
{
	public class JSTools
	{

		public static string Text(JToken token, string name)
		{
			if (!(token is JObject)) return "";
			JObject item = token as JObject;
			if (item == null) return "";
			var p = item[name];
			if (p == null) return "";
			return p.ToString();
		}

		public static bool IsTrue(JToken token, string name)
		{
			if (!(token is JObject)) return false;
			JObject item = token as JObject;
			if (item == null) return false;
			var p = item[name];
			if (p == null) return false;
			return p.Value<bool>();
		}

		internal static JToken ParseSafe(string value)
		{
			try
			{
				var res = JToken.Parse(value);
				return res;
			} catch
			{
				return null;
			}
		}

		public static long Long(JToken token, string name)
		{
			if (!(token is JObject)) return 0;
			JObject item = token as JObject;
			if (item == null) return 0;
			var p = item[name];
			if (p == null) return 0;
			return p.Value<long>();
		}

		public static void appendDict(JObject target, Dictionary<string, string> data)
		{
			if (target == null) return;
			foreach (string key in data.Keys)
			{
				target.Add(new JProperty(key, data[key]));
			}
		}

		public static void mergeObject(JObject target, JObject data)
		{
			if (target == null) return;
			if (data == null) return;
			foreach (var item in data)
			{
				target.Add(item.Key, item.Value);
			}
		}

		public static Dictionary<string, string> objectToDict(JObject data)
		{
			var res = new Dictionary<string, string>();
			foreach (var item in data)
			{
				if (item.Value is JArray) throw new ApplicationException("Dictionary does not support arrays");
				if (item.Value is JObject)
				{
					var sublist = objectToDict(item.Value as JObject);
					foreach (string skey in sublist.Keys)
					{
						res.Add(item.Key + "/" + skey, sublist[skey]);
					}
					continue;
				}

				res.Add(item.Key, item.Value.ToString());

			}
			return res;
		}

	}
}
