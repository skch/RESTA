#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestUseCases.Tools
{
	public class Terminal
	{

		protected static void writeErrorText(string etxt)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(etxt);
			Console.ResetColor();
		}

		protected static void writeWarningText(string etxt)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Console.WriteLine(etxt);
			Console.ResetColor();
		}

		public static bool WriteError(Exception ex, string msg = "")
		{
			writeErrorText(String.Format("{0} {1}", msg, ex.Message));
			Console.WriteLine(ex.StackTrace);
			return false;
		}

		public static bool WriteError(string txt)
		{
			writeErrorText(txt);
			return false;
		}

		public static bool WriteError(string msg, params object[] data)
		{
			string txt = String.Format(msg, data);
			writeErrorText(txt);
			return false;

		}

		internal static bool WriteWarning(string txt)
		{
			writeWarningText(txt);
			return false;
		}
	}
}
