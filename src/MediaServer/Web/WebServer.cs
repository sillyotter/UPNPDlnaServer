using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using MediaServer.Configuration;
using MediaServer.Utility;

namespace MediaServer.Web
{
    abstract class WebServer
    {
        private const int HandlerCount = 10;
        private readonly Regex _requestMatcher = new Regex(@"([A-Za-z]+) ([^ \t]+) \S+", RegexOptions.IgnoreCase);
        private readonly TcpListener _listener = new TcpListener(IPAddress.Any, Settings.Instance.QueryPort);
        private readonly BlockingQueue<TcpClient> _clientQueue = new BlockingQueue<TcpClient>(HandlerCount);
        private Thread _listenerThread;
        private readonly List<Thread> _handlers = new List<Thread>();

        private readonly List<KeyValuePair<String, BaseRequestHandler>> _requestHandlers = new List<KeyValuePair<string, BaseRequestHandler>>();

		public void Start()
		{
			lock (this)
			{

				if (_listenerThread != null) return;

				_listenerThread = new Thread(
					() =>
						{
							try
							{
								_listener.Start(HandlerCount);
								while (true)
								{
									var client = _listener.AcceptTcpClient();
									
									_clientQueue.Enqueue(client);
								}
							}
							catch (Exception)
							{
							}

						});

				for (var i = 0; i < HandlerCount; ++i)
				{
					_handlers.Add(
						new Thread(
							() =>
								{
									try
									{
										while (true)
										{
											var client = _clientQueue.Dequeue();
											try
											{
												client.SendBufferSize = 64*1024;
												client.LingerState = new LingerOption(false, 0);
												client.ReceiveTimeout = 500;

												ConnectionHandler(client);
											}
											catch (Exception e)
											{
												Logger.Instance.Exception("Error while handling socket", e);
											}
											finally
											{
												client.Close();
											}
										}
									}
									catch (QueueDisabledException)
									{
									}
								}));
				}

				_handlers.ForEach(item => item.Start());
				_listenerThread.Start();
			}
		}

    	public void Stop()
        {
			lock (this)
			{
				if (_listenerThread != null)
				{
					_listener.Stop();
					_clientQueue.Enabled = false;
					_listenerThread.Join();

					_handlers.ForEach(item => item.Join());
					_handlers.Clear();

					_listenerThread = null;
				}
			}
        }

        public IList<KeyValuePair<String, BaseRequestHandler>> RequestHandlers
        {
            get
            {
                return _requestHandlers;
            }
        }

        private void ConnectionHandler(TcpClient client)
        {
			//Logger.Instance.Debug("Got Connection");
            using(var ns = client.GetStream())
            using (var tr = new UnbufferedStreamReader(ns))
            using (var tw = new StreamWriter(ns))
            {
                tw.NewLine = "\r\n";
                tw.AutoFlush = true;

                var cmd = "";
                Uri uri = null;
                var headers = new Dictionary<string, string>();

				var line = tr.ReadLine();
				//Logger.Instance.Debug("Header = " + line);
				while (!String.IsNullOrEmpty(line))
				{
					var match = _requestMatcher.Match(line);
					if (match.Success)
					{
						//Logger.Instance.Debug("Found the request line");
						cmd = match.Groups[1].Value;
						uri = new Uri("http://" + client.Client.LocalEndPoint + match.Groups[2].Value, UriKind.Absolute);
						break;
					}
					line = tr.ReadLine();
				}

				line = tr.ReadLine();
				while (!String.IsNullOrEmpty(line))
				{
					var split = line.Split(new[] { ':' }, 2);
					if (split.Length == 2)
					{
						headers.Add(split[0].Trim(), split[1].Trim());
					}
					line = tr.ReadLine();
                }

                if (uri != null && !String.IsNullOrEmpty(cmd))
                {
					  var processResponse =
                        _requestHandlers.First(item => Regex.Match(uri.OriginalString, item.Key).Success);

                    if (!String.IsNullOrEmpty(processResponse.Key))
                    {
                        var handler = processResponse.Value;
                        handler.ProcessRequest(client.Client.LocalEndPoint, client.Client.RemoteEndPoint, cmd, uri, headers, tr, tw);
                        return;
                    }
                }

				tw.WriteLine("HTTP/1.0 404 Not Found");
				tw.WriteLine();
				Logger.Instance.Warn("Received request for '{0}' but found no handler to process it", uri);                   
            }
        }
    }
}
