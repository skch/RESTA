using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestUseCases.Rest
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

		public static long Long(JToken token, string name)
		{
			if (!(token is JObject)) return 0;
			JObject item = token as JObject;
			if (item == null) return 0;
			var p = item[name];
			if (p == null) return 0;
			return p.Value<long>();
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
