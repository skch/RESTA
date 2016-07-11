#region Source code license
/* RESTfull API Automated Testing tool
 * Source:    https://github.com/skch/RESTA
 * Author:    skch@usa.net
This is a free software (MIT license) */
#endregion
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace nielsen.se.rest
{
	public class RestResponse
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		public bool Success = false;
		public string ContentType = "";
		public string RawData = "";
		public string CleanData = "";
		public int HttpCode = 0;

		public string Message = "";
		public string RedirectUrl = "";

		public JObject jdata;
		public JArray jlist;
		public string Code;
		public TimeSpan Duration;

		public void UpdateFrom(HttpWebResponse response)
		{

			HttpCode = (int)response.StatusCode;
			ContentType = response.ContentType;

			Stream dataStream = response.GetResponseStream();
			StreamReader reader = new StreamReader(dataStream);
			RawData = reader.ReadToEnd();
			reader.Close();
			dataStream.Close();
			response.Close();
		}


		public void ParseResponse()
		{
			Message = "";
			jdata = null;
			jlist = null;
			Code = "";

			if (RawData.Trim().StartsWith("<"))
			{
				logger.Warn("Expecting JSON, but getting XML or HTML instead");
				if (!parseAsXml()) parseAsHtml();
				return;
			}

			if (Success) parseSuccess(); else parseError();


		}

		private void parseSuccess()
		{
			if (String.IsNullOrWhiteSpace(RawData))
			{
				logger.Error("A successfull call returns empty result");
				Success = false;
				return;
			}
			if (RawData == "null")
			{
				logger.Error("A successfull call returns JSON null result");
				Success = false;
				return;
			}
			logger.Trace(RawData);

			var jresponse = JObject.Parse(RawData);
			JToken jd = jresponse;
			Code = JTools.Attr(jresponse, "Code");
			if (!String.IsNullOrEmpty(Code))
			{
				Message = JTools.Attr(jresponse, "Message");
				RedirectUrl = JTools.Attr(jresponse, "RedirectUrl");
				jd = jresponse.GetValue("Data");
				if (!jresponse.TryGetValue("Data", out jd))
				{
					logger.Error("The expected element 'Data' was not found in the response");
					Success = false;
					return;
				}
			}
			else
			{
				logger.Warn("The JSON response is not in standard form");
			}
			if (jd is JArray) jlist = jd as JArray;
			if (jd is JObject) jdata = jd as JObject;
			Success = (Code == "200") || (Code == "1000");
		}


		private void parseError()
		{
			logger.Debug("Parsing the following error:\n{0}", RawData);
		}

		private bool parseAsXml()
		{
			try
			{
				var res = XElement.Parse(RawData);
				logger.Debug("Parsed result as XML: {0}", res.Name.LocalName);
				logger.Trace(RawData);
				return true;

			}
			catch
			{
				return false;
			}
		}

		private void parseAsHtml()
		{
			if (RawData.Contains("<i>Runtime Error</i>"))
			{
				logger.Error("Run-time error on REST server (HTTP)");
				return;
			}

			if (RawData.Contains("<i>The resource cannot be found.</i>"))
			{
				logger.Error("Invalid resource URL (HTML)");
				return;
			}

			logger.Error("Unknown reponse in HTML format:\n{0}", RawData);
		}

		public XElement AsXml()
		{
			var xres = new XElement("response");
			xres.Add(new XAttribute("code", HttpCode));
			xres.Add(new XAttribute("type", ContentType));
			if (!String.IsNullOrEmpty(Message)) xres.Add(new XAttribute("message", Message));
			xres.Add(new XAttribute("duration", String.Format("{0}:{1}.{2}", Duration.Minutes, Duration.Seconds, Duration.Milliseconds)));
			if (!String.IsNullOrEmpty(RawData)) xres.Add(new XElement("data", RawData));
			return xres;
		}
	}
}
