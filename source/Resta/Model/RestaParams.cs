#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model
{
	public class RestaParams
	{
		public bool needHelp = false;
		public string cmd = "";
		public string bookName = "";
		public string scriptPath = "";
		public string outputPath = "";
		public string inputPath = "";
		public string schemaPath = "";
		public string environmentName = "";
		public bool keepSuccess = false;
		public bool verbose = false;
		public bool responseHeader = false;
		public bool failFast = false;
		public bool createNewBook = false;
	}
}