using Newtonsoft.Json.Linq;
using RestUseCases.Rest;
using skch.rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases
{
	public class SequenceStatus
	{
		private string errorMessage = "";

		public int Result = 0;
		public string EnvName = "";
		public string CasesPath = "";

		public bool toBreakOnFail = true;

		public XElement XReport = null;
		public Dictionary<string, JObject> indexContext = new Dictionary<string, JObject>();
		public List<XElement> Operations = new List<XElement>();
		public Dictionary<string, string> listHeaders = new Dictionary<string, string>();
		public Dictionary<string, string> headers = new Dictionary<string, string>();


		// Current Test Case
		internal XElement currentCase = null;
		public RestRequest input;
		public RestResponse Response;

		public string rtype
		{
			get { return XTools.Attr(currentCase, "type");  }
		}

		public string Id
		{
			get { return XTools.Attr(currentCase, "id"); }
		}


		// ----------------------------------------------------
		public bool HasErrors
		{
			get { return Result > 0; }
		}

		// ----------------------------------------------------
		public void reset()
		{
			errorMessage = "";
			Result = 0;
		}

		// ----------------------------------------------------
		public SequenceStatus setException(Exception ex, string msg = "")
		{
			errorMessage = String.Format("Exception: {0}. {1}", ex.Message, msg);
			Result = 2;
			return this;
		}

		// ----------------------------------------------------
		public SequenceStatus setError(string msg)
		{
			errorMessage = msg;
			Result = 1;
			return this;
		}
	}
}
