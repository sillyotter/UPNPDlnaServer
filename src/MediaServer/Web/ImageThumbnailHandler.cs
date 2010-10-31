using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Media.Nodes;

namespace MediaServer.Web
{
	class ImageThumbnailHandler : BaseRequestHandler
	{		
		private const int BufferSize = 64 * 1024;

        public override void ProcessRequest(EndPoint localEndPoint, EndPoint remoteEndpoint, string method, Uri requestUri,
            IDictionary<string, string> headers, UnbufferedStreamReader inputStream, StreamWriter outputStream)
        {			
			var id = requestUri.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(id)) as ImageNode;

            if (node == null || String.IsNullOrEmpty(node.Thumbnail) || !File.Exists(node.Thumbnail))
            {
                Redirect(outputStream,
                         new Uri("http://" + localEndPoint + "/MediaServer/" + Settings.Instance.MusicIcon));
                return;
            }

            var requestedFile = node.Thumbnail;
			var fileInfo = new FileInfo(requestedFile);

            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Server: " + Settings.Instance.ServerName);
            outputStream.WriteLine("Content-Length: " + fileInfo.Length);
            outputStream.WriteLine("Content-Type: " + node.MimeType);
            outputStream.WriteLine("Date: " + DateTime.Now.ToUniversalTime().ToString("R"));
            outputStream.WriteLine();
						
			if (method == "HEAD") return;

			var buf = new byte[BufferSize];

			using (var fs = new FileStream(requestedFile, FileMode.Open,
			                               FileAccess.Read, FileShare.Read, BufferSize,
			                               FileOptions.SequentialScan))
			{
				try
				{
					do
					{
						var rc = fs.Read(buf, 0, BufferSize);
						if (rc == 0) break;
						outputStream.BaseStream.Write(buf, 0, rc);
					} while (true);
				}
				catch (Exception)
				{
				}
			}
		
		}
	}
}
