using Newtonsoft.Json.Linq;
using RestUseCases.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RestUseCases
{
	public class RunbookStatus
	{

		private string errorMessage = "";

		public int Result = 0;


		// Data
		public string BookFileName;
		public string EnvironmentName;
		public RunbookMetadata Book;
		public JObject envariables;
		public Dictionary<string, string> listHeaders = new Dictionary<string, string>();

		// ----------------------------------------------------
		public bool HasErrors
		{
			get { return Result > 0; }
		}

		// ----------------------------------------------------
		public RunbookStatus setException(Exception ex, string msg = "")
		{
			errorMessage = String.Format("Exception: {0}. {1}", ex.Message, msg);
			Result = 2;
			return this;
		}

		// ----------------------------------------------------
		public RunbookStatus setError(string msg)
		{
			errorMessage = msg;
			Result = 1;
			return this;
		}
	}
}
