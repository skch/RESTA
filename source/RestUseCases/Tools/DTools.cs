using RestUseCases.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Tools
{
	public class DTools
	{
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
