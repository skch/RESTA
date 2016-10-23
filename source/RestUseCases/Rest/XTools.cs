#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Rest
{
	public class XTools
	{

		public static string Attr(XElement parent, XName name)
		{
			if (parent == null) return "";
			var atr = parent.Attribute(name);
			if (atr == null) return "";
			return atr.Value;
		}

		public static int AttrInt(XElement parent, XName name, int def = 0)
		{
			if (parent == null) return def;
			var atr = parent.Attribute(name);
			if (atr == null) return def;
			int res;
			if (Int32.TryParse(atr.Value, out res)) return res;
			return def;
		}

	}
}
