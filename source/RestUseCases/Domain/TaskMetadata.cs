#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Schema;
using RestUseCases.Rest;
using RestUseCases.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases.Domain
{
	public class TaskMetadata
	{

		private XElement xmlHeader;
		private XElement xmlBody = null;
		public XElement xresult;

		public JSchema schema;

		public TaskMetadata(XElement xheader, XElement xbody)
		{
			xmlHeader = xheader;
			xmlBody = xbody;
			xresult = xmlBody.Element("result");
			if (xresult == null) return;
			schema = JSchema.Parse(xresult.Value);
		}

		public string Url
		{
			get {
				return XTools.Attr(xmlBody, "url");
			}
		}

		public string rtype
		{
			get { return XTools.Attr(xmlBody, "type"); }
		}

		public string Id
		{
			get { return XTools.Attr(xmlBody, "id"); }
		}

		public int HttpCode
		{
			get {
				if (xresult == null) return 0;
				return XTools.AttrInt(xresult, "code");
			}
		}

		public string ContentType
		{
			get
			{
				if (xresult == null) return "ERROR";
				return XTools.Attr(xresult, "type");
			}
		}

		public string Data
		{
			get
			{
				var xdata = xmlBody.Element("data");
				if (xdata == null) return "";
				return xdata.Value;
			}
		}

		// Returns superposition of headers from the test case and ist header
		public Dictionary<string, string> Headers
		{
			get
			{
				var res = new Dictionary<string, string>();
				var xheader = xmlBody.Element("header");
				if (xheader == null) return res;
				DTools.dictAppend(res, xheader, "id", "value");
				xheader = xmlHeader.Element("header");
				if (xheader == null) return res;
				DTools.dictAppend(res, xheader, "id", "value");
				return res;
			}
		}


	}
}
