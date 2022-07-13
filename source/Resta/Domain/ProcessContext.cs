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
		private string? _errorMessage;
		private string? _trace = "";
		
		public string? ErrorMessage => _errorMessage;
		public string ErrorDebug => _errorMessage + _trace;
		public bool HasErrors => !string.IsNullOrEmpty(_errorMessage);

		public T SetError<T>(T value, string? msg)
		{
			_errorMessage = msg;
			return value;
		}
		
		public T SetErrorNull<T>(string? msg)
		{
			_errorMessage = msg;
			return default!;
		}
		
		public T SetError<T>(T value, string msg, Exception ex)
		{
			_errorMessage = $"Exception: {msg}. {ex.Message}";
			_trace = ex.StackTrace;
			return value;
		}
		
		public T? SetErrorNull<T>(string msg, Exception ex)
		{
			_errorMessage = $"Exception: {msg}. {ex.Message}";
			_trace = ex.StackTrace;
			return default!;
		}

	}
}