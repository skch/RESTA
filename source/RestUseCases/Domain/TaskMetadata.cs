#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
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
		public string ErrorMessage = "";

		public TaskMetadata(XElement xheader, XElement xbody)
		{
			xmlHeader = xheader;
			xmlBody = xbody;
			xresult = xmlBody.Element("result");
			if (xresult == null) return;
			try
			{
				schema = JSchema.Parse(xresult.Value);
			}
			catch (JsonReaderException ex)
			{
				schema = null;
				ErrorMessage = String.Format("{0}: {1},{2}", ex.Message, ex.LineNumber, ex.LinePosition);
			}
		}

		public string Url
		{
			get {
				return XTools.Attr(xmlBody, "url");
			}
		}

		public string ContextVariable
		{
			get
			{
				return XTools.Attr(xmlHeader, "save");
			}
		}

		public bool IsDisabled
		{
			get
			{
				return XTools.Attr(xmlHeader, "disable") == "yes";
			}
		}

		public bool IsValid
		{
			get
			{
				// TODO: Validate Task metadata
				if (schema == null) return false;
				return true;
			}
		}

		public string rtype
		{
			get { return XTools.Attr(xmlBody, "type"); }
		}

		public string Id
		{
			get {
				var path = XTools.Attr(xmlHeader, "src");
				var parts = path.Split('/');
				return parts[parts.Length - 1];
			}
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
				CommonTools.dictAppend(res, xheader, "id", "value");
				xheader = xmlHeader.Element("header");
				if (xheader == null) return res;
				CommonTools.dictAppend(res, xheader, "id", "value");
				return res;
			}
		}


	}
}
