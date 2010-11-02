using System;
using System.Linq;
using System.Net;
using System.Web;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Media.Nodes;

namespace MediaServer.Web
{
	class MusicImageHandler : BaseRequestHandler
	{
        public override void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp)
		{			
			var id = req.Url.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(id)) as MusicNode;
			var mep = new IPEndPoint(req.LocalEndPoint.Address, Settings.Instance.MediaPort);

			if (node == null)
			{
			    resp.RedirectLocation = ("http://" + mep + "/MediaServer/" + HttpUtility.UrlPathEncode(Settings.Instance.MusicIcon));
				resp.StatusCode = (int)HttpStatusCode.MovedPermanently;
				resp.OutputStream.Close();
			   	return;	
			}
			
			var img = node.AlbumArt;
			var mt = img.MimeType;
			var dt = img.Data;
		
			resp.StatusCode = (int)HttpStatusCode.OK;
			resp.ContentLength64 = dt.Count();
			resp.ContentType = mt;

            resp.Headers.Add("Server", Settings.Instance.ServerName);
            resp.Headers.Add("Date", DateTime.Now.ToUniversalTime().ToString("R"));

			if (req.HttpMethod == "HEAD") return;

			try
			{
				var rawData = dt.ToArray();
				resp.OutputStream.Write(rawData, 0, rawData.Length);
				resp.OutputStream.Close();
			}
			catch(Exception)
			{
			}
		
		}
	}
}
