using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Media.Nodes;

namespace MediaServer.Web
{
	class MusicImageHandler : BaseRequestHandler
	{
		private static TagLib.IPicture LoadPictureFromImage(string requestedFile)
		{				
			if (!File.Exists(requestedFile))
			{
				return null;
			}
			
			try
			{
				var tagfile = TagLib.File.Create(requestedFile);
				var img = tagfile.Tag.Pictures.FirstOrDefault();
				return img;
			}
			catch (Exception)
			{
				return null;
			}
		}

        public override void ProcessRequest(EndPoint localEndPoint, EndPoint remoteEndpoint, string method, Uri requestUri,
            IDictionary<string, string> headers, UnbufferedStreamReader inputStream, StreamWriter outputStream)
		{			
			var id = requestUri.Query.Split('=')[1];
			var node = MediaRepository.Instance.GetNodeForId(new Guid(id)) as MusicNode;
			if (node == null)
			{
			    Redirect(outputStream, new Uri("http://" + localEndPoint + "/MediaServer/" + Settings.Instance.MusicIcon));
			   	return;	
			}
			
			var requestedFile = node.Location;
			var img = LoadPictureFromImage(requestedFile);
			
			if (img == null)
			{
			    Redirect(outputStream, new Uri("http://" + localEndPoint + "/MediaServer/" + Settings.Instance.MusicIcon));
                return;
			}
		
			var mt = img.MimeType;
			var dt = img.Data;
		
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Server: " + Settings.Instance.ServerName);
            outputStream.WriteLine("Content-Length: " + dt.Count());
            outputStream.WriteLine("Content-Type: " + mt);
            outputStream.WriteLine("Date: " + DateTime.Now.ToUniversalTime().ToString("R"));
            outputStream.WriteLine();

			if (method == "HEAD") return;

			try
			{
				var rawData = dt.ToArray();
				outputStream.BaseStream.Write(rawData, 0, rawData.Length);
			}
			catch(Exception)
			{
			}
		
		}
	}
}
