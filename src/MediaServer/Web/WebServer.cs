using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MediaServer.Configuration;

namespace MediaServer.Web
{
	abstract class WebServer
	{
        private readonly List<KeyValuePair<String, BaseRequestHandler>> _requestHandlers = new List<KeyValuePair<string, BaseRequestHandler>>();
		private HttpListener _listener;

		private void ContextHandler(IAsyncResult res)
		{
			var context = _listener.EndGetContext(res);

			_listener.BeginGetContext(ContextHandler, _listener);

			var req = context.Request;
			var resp = context.Response;

			resp.ProtocolVersion = req.ProtocolVersion;
			resp.KeepAlive = req.KeepAlive;
			resp.SendChunked = false;

			var processResponse = _requestHandlers.First(item => Regex.Match(req.Url.OriginalString, item.Key).Success);

			if (!String.IsNullOrEmpty(processResponse.Key))
			{
				var handler = processResponse.Value;
				handler.ProcessRequest(req, resp);
			}
			else
			{
				resp.StatusCode = (int)HttpStatusCode.NotFound;
			}
		}

		public void Start()
		{
			if (_listener == null)
			{
				_listener = new HttpListener {IgnoreWriteExceptions = true};

			    _listener.Prefixes.Add("http://+:" + Settings.Instance.QueryPort + "/");
				_listener.Start();

				_listener.BeginGetContext(ContextHandler, _listener);
			}
		}

		public void Stop()
		{
			if (_listener != null)
			{
				_listener.Stop();
				_listener.Close();
				_listener = null;
			}
		}

        public IList<KeyValuePair<String, BaseRequestHandler>> RequestHandlers
        {
            get
            {
                return _requestHandlers;
            }
        }
	}

}
