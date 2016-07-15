#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using RestUseCases.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestUseCases
{
	class Program
	{
		static void Main(string[] args)
		{

			Console.WriteLine("REST API Use Cases Testing Tool");
			string runBookFile = getParam(args, 1, "runbook.xml");
			string environmentFile = getParam(args, 2, "environment.xml");

			if (args.Length > 1)
			{
				var runBook = new TestRunBook();
				if (runBook.Load(args[0], args[1]))
					runBook.Execute();
				Console.WriteLine("Completed");
			}
			else
			{
				Console.WriteLine("  usage rtest runbook.xml env.xml");
			}

#if DEBUG
			Console.ReadLine();
#endif
		}

		private static string getParam(string[] plist, int pn, string value)
		{
			if (plist.Length > pn - 1) return plist[pn - 1];
			return value;
		}
	}
}
