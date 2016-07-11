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

namespace nielsen.se.rest
{
	public class RestRequest
	{
		public string Url = "";
		public Dictionary<string, string> header = new Dictionary<string, string>();
		public string ContentType = "application/json; charset=utf-8";
		public string Content = null;

		public XElement AsXml(string method)
		{
			var xres = new XElement("request");
			xres.Add(new XAttribute("url", Url));
			xres.Add(new XAttribute("method", method));
			return xres;
		}
	}
}
