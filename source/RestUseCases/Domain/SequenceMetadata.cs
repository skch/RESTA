#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using RestUseCases.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RestUseCases.Domain
{

	// The proxy class to parse sequence metadata from XML
	public class SequenceMetadata
	{

		private XElement xmlBody;

		public SequenceMetadata(XElement xbody)
		{
			xmlBody = xbody;
		}

		public string Id
		{
			get { return XTools.Attr(xmlBody, "id"); }
		}

		public JObject Context
		{
			get {
				var xdata = xmlBody.Element("context");
				if (xdata == null) return null;
				var data = JSTools.ParseSafe(xdata.Value);
				if (data == null) return null;
				if (data is JObject) return data as JObject;
				return null;
			}
		}

		public bool toSaveContext {
			get { return XTools.Attr(xmlBody, "showContext") == "yes"; }
		}

		public bool toBreakOnFail
		{
			get { return XTools.Attr(xmlBody, "break") == "yes"; }
		}
	

	}
}
