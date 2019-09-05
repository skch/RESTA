using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Tools
{
	public class CommonTools
	{

		public static JObject xmlToJson(XElement parent)
		{
			var res = new JObject();
			if (parent == null) return res;
			return xmlToJson(parent.Elements());
		}

		public static JObject xmlToJson(IEnumerable<XElement> list)
		{
			var res = new JObject();
			if (list != null)
				foreach (XElement xvar in list)
				{
					string key = XTools.Attr(xvar, "id");
					res.Add(new JProperty(key, XTools.Attr(xvar, "value")));
				}
			return res;
		}

		public static Dictionary<string, string> xmlToDict(XElement parent)
		{
			var res = new Dictionary<string, string>();
			if (parent == null) return res;
			return xmlToDict(parent.Elements());
		}

		public static Dictionary<string, string> xmlToDict(IEnumerable<XElement> list)
		{
			var res = new Dictionary<string, string>();
			if (list != null)
				foreach (XElement xvar in list)
				{
					string key = XTools.Attr(xvar, "id");
					res.Add(key, XTools.Attr(xvar, "value"));
				}
			return res;
		}

		public static void dictMerge(Dictionary<string, string> target, Dictionary<string, string> source)
		{
			foreach (string key in source.Keys)
			{
				if (target.ContainsKey(key)) target[key] = source[key]; else target.Add(key, source[key]);
			}
		}


		public static void dictAppend(Dictionary<string, string> target, XElement xlist, XName kname, XName vname)
		{
			foreach (XElement xitem in xlist.Elements())
			{
				string key = XTools.Attr(xitem, kname);
				string value = XTools.Attr(xitem, vname);
				if (target.ContainsKey(key)) target[key] = value; else target.Add(key, value);
			}
		}



	}
}
