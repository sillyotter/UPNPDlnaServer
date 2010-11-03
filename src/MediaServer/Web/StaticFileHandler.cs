using System;
using System.IO;
using System.Net;
using MediaServer.Configuration;

namespace MediaServer.Web
{
	class StaticFileHandler : BaseRequestHandler
	{
		private readonly string _ddtxt;
		private readonly string _cmtxt;
		private readonly string _cctxt;

		public StaticFileHandler()
		{
			_ddtxt = File.ReadAllText(Path.Combine(Settings.Instance.StaticResources, "MediaServer/DeviceDescription.xml"));
			_cmtxt = File.ReadAllText(Path.Combine(Settings.Instance.StaticResources, "MediaServer/ConnectionManager.xml"));
			_cctxt = File.ReadAllText(Path.Combine(Settings.Instance.StaticResources, "MediaServer/ContentDirectory.xml"));
		}

		private static void SendData(string data, HttpListenerResponse resp)
		{
				resp.ContentLength64 = data.Length;
				resp.ContentType = "text/xml";
				resp.StatusCode = (int)HttpStatusCode.OK;
				using (var tw = new StreamWriter(resp.OutputStream))
				{
					tw.Write(data);
				}
		}

        public override void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp)
		{
			if (req.Url.LocalPath.EndsWith("DeviceDescription.xml"))
			{
				SendData(_ddtxt, resp);
			}
			else if (req.Url.LocalPath.EndsWith("ConnectionManager.xml"))
			{
				SendData(_cmtxt, resp);
			}
			else if (req.Url.LocalPath.EndsWith("ContentDirectory.xml"))
			{
				SendData(_cctxt, resp);
			}
			else
			{
				resp.RedirectLocation = "http://" + req.LocalEndPoint.Address + ":" + Settings.Instance.MediaPort + req.Url.LocalPath;
				resp.StatusCode = (int)HttpStatusCode.MovedPermanently;
			}
			resp.OutputStream.Close();
		}
	}
}
