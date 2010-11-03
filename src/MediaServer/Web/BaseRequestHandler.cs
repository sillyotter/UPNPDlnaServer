using System.Net;

namespace MediaServer.Web
{
	abstract class BaseRequestHandler
	{
	    public abstract void ProcessRequest(HttpListenerRequest req, HttpListenerResponse resp);
	}
}
