using System;
using System.IO;
using System.Linq;
using MediaServer.Configuration;
using MediaServer.Media;
using MediaServer.SSDP;
using MediaServer.Utility;
using MediaServer.Web;
using System.Threading;

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
#if (WIN32)
			var configFileName = "Configuration.xml";
#else
			var configFileName = "MediaServer/Configuration.xml";
#endif
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

			Settings.Instance.LoadConfigurationFile(configFileName);

			var lighttpd = Lighttpd.Instance;
			lighttpd.Port = Settings.Instance.MediaPort;
			lighttpd.DocRoot = Settings.Instance.StaticResources;

			var itunesQuery = from item in Settings.Instance.iTunesFolders
				select Path.GetDirectoryName(item.Path);
			var iphotoQuery = from item in Settings.Instance.iPhotoFolders
				select Path.GetDirectoryName(item.Path);
			var imageQuery = from item in Settings.Instance.MediaFolders
				select item.Path;

			foreach (var item in imageQuery.Union(itunesQuery).Union(iphotoQuery))
			{
				lighttpd.UrlMapping[item] = item;
			}

#if (!WIN32)
			lighttpd.Start();
#endif
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

			lighttpd.Stop();
		}
	}
}
