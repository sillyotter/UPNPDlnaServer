using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Media.Nodes;
using MediaServer.Utility;
using Mono.Unix.Native;
using System.Web;

namespace MediaServer.Web
{
	internal class MediaHandler : BaseRequestHandler
	{
		private const int BufferSize = 64 * 1024;
		//private readonly Regex _rangeMatch = new Regex(@"bytes=(\d+)-(\d+)");

		#region Implementation of BaseRequestHandler

        public override void ProcessRequest(EndPoint localEndPoint, EndPoint remoteEndpoint, string method, Uri requestUri,
            IDictionary<string, string> headers, UnbufferedStreamReader inputStream, StreamWriter outputStream)
        {
			var strId = requestUri.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(strId)) as FileNode;
			var requestedFile = node != null ? node.Location : "";

			var location = "http://" + localEndPoint.ToString().Split(':')[0] + ":54321" + requestedFile.Substring(22);
			Redirect(outputStream, new Uri(location));
		}

		#endregion
	}
}
