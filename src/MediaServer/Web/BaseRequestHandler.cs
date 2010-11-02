using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;

namespace MediaServer.Web
{
	abstract class BaseRequestHandler
	{
	    public abstract void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp);
	}
}
