#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

namespace Resta.Model
{
	public class RestEnvironment
	{
		public string id;
		public string title;
		public readonly Dictionary<string, string> values = new Dictionary<string, string>();
		private readonly Random _randomEngine = new Random();
		
		public RestEnvironment()
		{
			id = "nan";
			title = "Undefined Environment";
		}

		public RestEnvironment(RestEnvironmentJson data)
		{
			id = data.id ?? string.Empty;
			title = data.title ?? string.Empty;
			if (data.values != null) values = data.values;
		}

		public void SetValue(string key, string value)
		{
			if (values.ContainsKey(key)) 
				values[key] = value; 
			else 
				values.Add(key, value);
		}
		public Dictionary<string, string> includingDynamic()
		{
			Dictionary<string, string> res = new Dictionary<string, string>(values);
			res.Add("$timestamp", getEpochTime());
			res.Add("$guid", Guid.NewGuid().ToString());

			int rint = _randomEngine.Next();
			res.Add("$randomInt", rint.ToString());
			
			// TODO: To add more dynamic variables
			return res;
		}
		
		private static string getEpochTime()
		{
			TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
			long epoch = (long)t.TotalSeconds;
			return $"{epoch}";
		}

	}
}