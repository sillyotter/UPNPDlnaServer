using System;
using System.Threading;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.SSDP;
using MediaServer.Utility;
using MediaServer.Web;

#if (!WIN32)
using Mono.Unix;
using Mono.Unix.Native;
#endif

namespace MediaServer
{
	class Program
	{
		static void Main(string[] args)
		{
			
			var configFileName = "MediaServer/Configuration.xml";
			string logfileName = null;
			if (args.Length >= 1)
			{
				configFileName = args[0];
			}
			if (args.Length == 2)
			{
				logfileName = args[1];
			}
			
			if (!String.IsNullOrEmpty(logfileName))
			{
				Logger.Instance.Initialize(logfileName, LogLevel.Debug);
			}
			else
			{
				Logger.Instance.LogLogLevel = LogLevel.Debug;
			}

			//Logger.Instance.Info("Starting");			
			
			Settings.Instance.LoadConfigurationFile(configFileName);
			MediaRepository.Instance.Initialize();

			var upnplistener = new UpnpMediaServerMessageHandler();
			var upnpsoapserver = new UpnpWebServer();

			upnpsoapserver.Start();
			upnplistener.Start();

#if (!WIN32)
			var signals = new UnixSignal[]{ 
				new UnixSignal(Signum.SIGTERM),
				new UnixSignal(Signum.SIGINT)
			};
			UnixSignal.WaitAny(signals, -1);
#else
			Thread.Sleep((int)TimeSpan.FromHours(60).TotalMilliseconds);
#endif

			upnplistener.Stop();
			upnpsoapserver.Stop();
		}
	}
}
