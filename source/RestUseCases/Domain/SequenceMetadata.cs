#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
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

		public bool toSaveContext {
			get { return XTools.Attr(xmlBody, "showContext") == "yes"; }
		}
	}
}
