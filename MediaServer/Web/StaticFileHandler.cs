using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.Utility;

namespace MediaServer.Web
{
	class StaticFileHandler : BaseRequestHandler
	{
		private const int BufferSize = 16 * 1024;

        public override void ProcessRequest(EndPoint localEndPoint, EndPoint remoteEndpoint, string method,
            Uri requestUri, IDictionary<string, string> headers, UnbufferedStreamReader inputStream, StreamWriter outputStream)
		{
			var requestedFile = requestUri.LocalPath;
			switch (requestedFile)
			{
				case "/":
					requestedFile = Settings.Instance.StaticResources + "/index.html";
					break;
				case "/MediaServer/ThisIsNotARealIcon.png":
                    Redirect(outputStream, new Uri( "http://" + localEndPoint + "/MediaServer/" + Settings.Instance.ServerIcon));
					return;
				default:
					requestedFile = Settings.Instance.StaticResources + requestedFile;
					break;
			}

			if (String.IsNullOrEmpty(requestedFile) || !File.Exists(requestedFile))
			{
				Logger.Instance.Warn("Received request for static file '{0}' but couldn't locate it.", requestedFile);
			    NotFound(outputStream);
				return;
			}

			var fi = new FileInfo(requestedFile);


            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Server: " + Settings.Instance.ServerName);
            outputStream.WriteLine("Content-Length: " + fi.Length );
            outputStream.WriteLine("Date: " + DateTime.Now.ToUniversalTime().ToString("R"));
			outputStream.WriteLine("Content-Type: " + MimeTypeLookup.GetMimeType(requestedFile));
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
						if (rc == 0) 
						{
							break;
						}
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
