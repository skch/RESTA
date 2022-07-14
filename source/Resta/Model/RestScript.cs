#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion

using System.Collections.Generic;

namespace Resta.Model
{
	public class RestScript
	{
		public string id;
		public string title;
		public readonly ApiTaskSettings shared = new ApiTaskSettings();
		public readonly List<RestTask> tasks = new List<RestTask>();

		public RestScript(RestScriptJson data)
		{
			id = data.id ?? string.Empty;
			title = data.title ?? string.Empty;
			if (data.shared != null)
			{
				if (data.shared.timeout != null) shared.timeout = (int)data.shared.timeout;
				if (data.shared.header != null) shared.header = data.shared.header;
			}
			if (data.tasks != null)
			{
				foreach (var task in data.tasks)
				{
					if (task.disabled) continue;
					tasks.Add(new RestTask(task));
				}
			}
		}
	}
}