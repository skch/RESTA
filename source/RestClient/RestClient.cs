#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace nielsen.se.rest
{
	public class RestClient
	{
		public bool toTraceDataReceived = true;

		private static Logger logger = LogManager.GetCurrentClassLogger();

		// ====================================================================
		// Execute raw get request
		public RestResponse get(string rtype, RestRequest input)
		{
			RestResponse res = new RestResponse();

			WebRequest request = WebRequest.Create(input.Url);
			request.Method = rtype;
			request.ContentType = input.ContentType;
			foreach (string key in input.header.Keys)
			{
				request.Headers[key] = input.header[key];
			}

			WebResponse response = null;
			try
			{
				var start = DateTime.Now;
				response = request.GetResponse();
				res.Duration = DateTime.Now - start;
				if (response is HttpWebResponse) res.UpdateFrom(response as HttpWebResponse);
				var hresponse = (HttpWebResponse)response;

				logger.Debug(hresponse.StatusDescription);
				res.Success = true;
			}
			catch (WebException wex)
			{
				logger.Error("Error {0} while GET {1}", wex.Status, input.Url);
				if (wex.Response is HttpWebResponse) res.UpdateFrom(wex.Response as HttpWebResponse);
				res.Message = wex.Message;
				res.Success = false;
			}

			return res;
		}

		public RestResponse post(string rtype, RestRequest input)
		{
			RestResponse res = new RestResponse();

			WebRequest request = WebRequest.Create(input.Url);
			request.Method = rtype;
			request.ContentType = input.ContentType;
			foreach (string key in input.header.Keys)
			{
				request.Headers[key] = input.header[key];
			}

			// Create POST data and convert it to a byte array.
			byte[] byteArray = Encoding.UTF8.GetBytes(input.Content);
			//request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();

			WebResponse response = null;
			try
			{
				var start = DateTime.Now;
				response = request.GetResponse();
				res.Duration = DateTime.Now - start;
				if (response is HttpWebResponse) res.UpdateFrom(response as HttpWebResponse);
				var hresponse = (HttpWebResponse)response;

				logger.Debug(hresponse.StatusDescription);
				res.Success = true;
			}
			catch (WebException wex)
			{
				logger.Error("Error {0} while POST {1}", wex.Status, input.Url);
				if (wex.Response is HttpWebResponse) res.UpdateFrom(wex.Response as HttpWebResponse);
				res.Message = wex.Message;
				res.Success = false;
			}

			return res;
		}


	}
}
