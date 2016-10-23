#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using RestUseCases.Domain;
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
		public string errorMessage = "";

		public int Result = 0;
		public string EnvName = "";
		public string CasesPath = "";

		public bool toBreakOnFail = true;

		public XElement XReport = null;
		public SequenceMetadata metadata = null;
		public List<TaskMetadata> Operations = new List<TaskMetadata>();
		public JObject context;
		public Dictionary<string, string> listHeaders = new Dictionary<string, string>();
		public Dictionary<string, string> headers = new Dictionary<string, string>();


		// Current Test Case
		internal TaskMetadata currentCase = null;
		internal XElement xTestReport = null;
		public RestRequest input;
		public RestResponse Response;

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
			Result = 3;
			return this;
		}

		// ----------------------------------------------------
		public SequenceStatus setWarning(string msg)
		{
			errorMessage = msg;
			Result = 1;
			return this;
		}

		// ----------------------------------------------------
		public SequenceStatus setError(string msg)
		{
			errorMessage = msg;
			Result = 2;
			return this;
		}
	}
}
