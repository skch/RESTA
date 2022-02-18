#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System;
using System.Threading.Tasks;
using Resta.Model;

namespace Resta.Domain
{
	public class ProcessContext
	{
		private string errorMessage;
		private string trace = "";
		
		public string ErrorMessage => errorMessage;
		public string ErrorDebug => errorMessage + trace;
		public bool HasErrors => !string.IsNullOrEmpty(errorMessage);

		public T SetError<T>(T value, string msg)
		{
			errorMessage = msg;
			return value;
		}
		
		public T SetError<T>(T value, string msg, Exception ex)
		{
			errorMessage = $"Exception: {msg}. {ex.Message}";
			trace = ex.StackTrace;
			return value;
		}

	}
}