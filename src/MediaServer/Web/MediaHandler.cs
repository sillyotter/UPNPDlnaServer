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
		#region Implementation of BaseRequestHandler

        public override void ProcessRequest(EndPoint localEndPoint, EndPoint remoteEndpoint, string method, Uri requestUri,
            IDictionary<string, string> headers, UnbufferedStreamReader inputStream, StreamWriter outputStream)
        {
			var strId = requestUri.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(strId)) as FileNode;
			var requestedFile = node != null ? node.Location : "";

			var location = "http://" + localEndPoint.ToString().Split(':')[0] + ":" + Settings.Instance.MediaPort + requestedFile;
			Redirect(outputStream, new Uri(location));
		}

		#endregion
	}
}
