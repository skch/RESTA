using Newtonsoft.Json.Linq;
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
	public class RunbookMetadata
	{

		private XElement xmlBook;
		private XElement xmlEnvironment;

		public RunbookMetadata(XElement xbook, XElement xenv)
		{
			xmlBook = xbook;
			xmlEnvironment = xenv;
		}

		public string EnvironmentName
		{
			get { return XTools.Attr(xmlEnvironment, "name");  }
		}

		public Dictionary<string, string> Data
		{
			get {
				var res = new Dictionary<string, string>();
				var xvariables = xmlEnvironment.Element("variables");
				if (xvariables != null)
					DTools.dictAppend(res, xvariables, "id", "value");
				xvariables = xmlBook.Element("variables");
				if (xvariables != null)
					DTools.dictAppend(res, xvariables, "id", "value");
				return res;
			}
		}

		public IEnumerable<XElement> Sequence
		{
			get
			{
				return xmlBook.Elements("sequence");
			}
		}

		public Dictionary<string, string> Headers
		{
			get
			{
				var res = new Dictionary<string, string>();
				var xheaders = xmlEnvironment.Element("header-all");
				if (xheaders != null)
				{
					DTools.dictAppend(res, xheaders, "id", "value");
				}
				xheaders = xmlBook.Element("header-all");
				if (xheaders != null)
				{
					DTools.dictAppend(res, xheaders, "id", "value");
				}
				return res;
			}
		}


		public string Validate()
		{
			if (xmlEnvironment == null) return "Missing environment";
			if (xmlBook == null) return "Missing runbook";
			if (Sequence == null) return "Missing runbook sequence";
			// TODO: add more validations
			return "";
		}
	}
}
