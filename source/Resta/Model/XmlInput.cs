
#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Xml;
using System.Xml.Linq;

namespace Resta.Model
{
	public class XmlInput
	{
		public string XML;

		public XmlInput(string data)
		{
			var doc = XDocument.Parse(data);
			
			StringWriter sw = new StringWriter();
			XmlTextWriter xw = new XmlTextWriter(sw);
			xw.Formatting = Formatting.Indented;
			doc.WriteTo(xw);
			XML = sw.ToString();
		}
	}
}