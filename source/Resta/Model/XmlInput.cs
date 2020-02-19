using System;
using System.IO;
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