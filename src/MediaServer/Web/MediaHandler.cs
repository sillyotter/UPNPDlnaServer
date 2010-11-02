using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Media.Nodes;
using MediaServer.Utility;

namespace MediaServer.Web
{
	internal class MediaHandler : BaseRequestHandler
	{
        public override void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {
			var strId = req.Url.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(strId)) as FileNode;
			var requestedFile = node != null ? node.Location : "";

			var location = "http://" + req.LocalEndPoint.Address + ":" + Settings.Instance.MediaPort + HttpUtility.UrlPathEncode(requestedFile);

			resp.RedirectLocation = location;
			resp.StatusCode = (int)HttpStatusCode.MovedPermanently;
			resp.OutputStream.Close();
		}
	}
}
