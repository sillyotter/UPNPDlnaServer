using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace MediaServer.Web
{
	abstract class BaseRequestHandler
	{
	    public abstract void ProcessRequest(
            EndPoint localEndpoint, 
			EndPoint remoteEndpoint,
            string method, 
            Uri resource, 
            IDictionary<string, string> headers,
            UnbufferedStreamReader inputStream, 
            StreamWriter outputStream);

        public void Redirect(TextWriter outputStream, Uri location)
        {
            outputStream.WriteLine("HTTP/1.0 301 Moved Permanently");
            outputStream.WriteLine("Location: " + HttpUtility.UrlPathEncode( location.ToString()));
            outputStream.WriteLine();
        }

        public void NotFound(TextWriter outputStream)
        {
            outputStream.WriteLine("HTTP/1.0 404 Not Found");
            outputStream.WriteLine();
        }
	}
}
