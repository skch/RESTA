using RestUseCases.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
