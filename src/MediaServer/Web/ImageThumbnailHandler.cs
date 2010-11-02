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

        public override void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp)
        {			
			var id = req.Url.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(id)) as ImageNode;

            if (node == null || String.IsNullOrEmpty(node.Thumbnail) || !File.Exists(node.Thumbnail))
            {
                resp.Redirect("http://" + req.LocalEndPoint + "/MediaServer/" + HttpUtility.UrlPathEncode(Settings.Instance.ImageIcon));
				resp.OutputStream.Close();
                return;
            }

            var requestedFile = node.Thumbnail;
			var fileInfo = new FileInfo(requestedFile);

			resp.StatusCode = (int)HttpStatusCode.OK;
			resp.ContentLength64 = fileInfo.Length;
			resp.ContentType = node.MimeType;
            resp.Headers.Add("Server", Settings.Instance.ServerName);
            resp.Headers.Add("Date", DateTime.Now.ToUniversalTime().ToString("R"));
						
			if (req.HttpMethod == "HEAD") return;

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
						resp.OutputStream.Write(buf, 0, rc);
					} while (true);
				}
				catch (Exception)
				{
				}
				finally
				{
					resp.OutputStream.Close();
				}
			}
		
		}
	}
}
