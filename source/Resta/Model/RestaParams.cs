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
		public bool NeedHelp = false;
		public string Cmd = "";
		public string BookName = "";
		public string ScriptPath = "";
		public string OutputPath = "";
		public string InputPath = "";
		public string SchemaPath = "";
		public string EnvironmentName = "";
		public bool KeepSuccess = false;
	}
}