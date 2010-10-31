using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MediaServer.Utility;

namespace MediaServer.Configuration
{
	sealed class Settings
	{
		private static readonly XNamespace Ns = "urn:schemas-upnp-org:device-1-0";

		#region Singleton

		private static readonly Settings SingletonInstance = new Settings();

		static Settings()
		{
		}

		#endregion

		private readonly IDictionary<string,string> _mediaFolders = new Dictionary<string,string>();
		private readonly IDictionary<string,string> _itunesLibraries = new Dictionary<string,string>();
		private readonly IDictionary<string,string> _iphotoLibraries = new Dictionary<string,string>();
		private readonly IList<FlickrConfigElement> _flickr = new List<FlickrConfigElement>();
		private readonly IList<PicasaConfigElement> _picasa = new List<PicasaConfigElement>();
		private readonly FileSystemWatcher _watcher = new FileSystemWatcher();

		public event EventHandler<EventArgs> ConfigurationChanged;

		private Settings()
		{
			MovieIcon = "Movie2.png";
			ImageIcon = "Pictures2.png";
			MusicIcon = "Music2.png";
			ServerIcon = "Home2.png";	
		}

		public static Settings Instance
		{
			get
			{
				return SingletonInstance;
			}
		}

		public void LoadConfigurationFile(string filename)
		{
			//Logger.Instance.Info("Loading configuration file {0}", filename);
			
			ProcessFile(filename);

			_watcher.IncludeSubdirectories = false;
			_watcher.Path = Path.GetDirectoryName(filename);
			_watcher.Filter = Path.GetFileName(filename);
			_watcher.EnableRaisingEvents = true;
			_watcher.NotifyFilter = NotifyFilters.LastWrite;
			_watcher.Changed += OnChanged;	
		}

		private void FireConfigurationChanged()
		{
			var temp = ConfigurationChanged;
			if (temp != null)
			{
				temp(this, EventArgs.Empty);
			}
		}

		private void ProcessFile(string filename)
		{
			_mediaFolders.Clear();
			_iphotoLibraries.Clear();
			_itunesLibraries.Clear();

			var configDoc = XDocument.Load(filename);

			LoadPortSettings(configDoc);
			LoadStaticResourceSettings(configDoc);
			LoadMediaFolders(configDoc);
			LoadiTunesFiles(configDoc);
			LoadiPhotoFiles(configDoc);
			LoadIcons(configDoc);
			LoadFlickrSettings(configDoc);
			LoadPicasaSettings(configDoc);
			GetDeviceIdAndName();
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			Logger.Instance.Info("Configuration file was changed");
			_watcher.EnableRaisingEvents = false;
			ProcessFile(e.FullPath);
			FireConfigurationChanged();
			_watcher.EnableRaisingEvents = true;
		}

		private void GetDeviceIdAndName()
		{
			var deviceFileName = StaticResources + "/MediaServer/DeviceDescription.xml";
				
			var deviceDoc = XDocument.Load(deviceFileName);
				
			var deviceQuery = from item in deviceDoc.Descendants(Ns + "device")
			                  let udn = item.Element(Ns + "UDN")
			                  where udn != null
			                  select udn;

			var resultNode = deviceQuery.FirstOrDefault();
			if (resultNode == null) throw new InvalidOperationException("Corrupt Device Description.");
			
			var result = resultNode.Value.Trim();

			if (result.StartsWith("uuid:"))
			{
				result = result.Substring(5);
				DeviceId = new Guid(result);
			}
			else
			{
				DeviceId = Guid.NewGuid();
				resultNode.Value = "uuid:" + DeviceId.ToString().ToLower();
				deviceDoc.Save(deviceFileName);
			}

			var nameQuery = from item in deviceDoc.Descendants(Ns + "device")
			                let nameNode = item.Element(Ns + "friendlyName")
			                where nameNode != null
			                select nameNode.Value.Trim();

			var name = nameQuery.FirstOrDefault();
			FriendlyName = name ?? "Media Server";	
		}

		private void LoadIcons(XContainer configDoc)
		{
			var iconQuery = from item in configDoc.Descendants("Icons")
			                let movie = item.Attribute("movie")
			                let movieName = movie == null ? "Movie2.png" : ((string)movie).Trim()
			                let music = item.Attribute("music")
			                let musicName = movie == null ? "Music2.png" : ((string)music).Trim()
			                let image = item.Attribute("image")
			                let imageName = image == null ? "Movie2.png" : ((string)image).Trim()
			                let server = item.Attribute("server")
			                let serverName = server == null ? "Home2.png" : ((string)server).Trim()
			                select new { Movie = movieName, Music = musicName, Image = imageName, Server = serverName };
				
			var icons = iconQuery.FirstOrDefault();
			
			if (icons == null) return;
			
			MovieIcon = icons.Movie;
			MusicIcon = icons.Music;
			ImageIcon = icons.Image;
			ServerIcon = icons.Server;
		}

		private void LoadiPhotoFiles(XContainer configDoc)
		{
			LoadNamedMediaEntrys(configDoc, "iPhoto", _iphotoLibraries);
		}

		private void LoadiTunesFiles(XContainer configDoc)
		{
			LoadNamedMediaEntrys(configDoc, "iTunes", _itunesLibraries);
		}
		
		private void LoadMediaFolders(XContainer configDoc)
		{
			LoadNamedMediaEntrys(configDoc, "Dir", _mediaFolders);
		}

		private class KeyComparer : IEqualityComparer<KeyValuePair<string, string>>
		{
			#region Implementation of IEqualityComparer<KeyValuePair<string,string>>

			public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
			{
				return x.Key.Equals(y.Key);
			}

			public int GetHashCode(KeyValuePair<string, string> obj)
			{
				return obj.Key.GetHashCode();
			}

			#endregion
		}

		private static void LoadNamedMediaEntrys(XContainer configDoc, string itemname, ICollection<KeyValuePair<string, string>> target)
		{
			var query = (from store in configDoc.Descendants("Storage")
			             from media in store.Elements("Media")
			             from tunes in media.Elements(itemname)
			             let nameNode = tunes.Attribute("name")
			             let name = nameNode == null ? "" : ((string)nameNode).Trim()
			             let path = ((string)tunes).Trim()
			             where !String.IsNullOrEmpty(name) && !String.IsNullOrEmpty(path)
			             select new KeyValuePair<string, string>(name, path)).Distinct(new KeyComparer());

			foreach (var item in query)
			{
				target.Add(item);
			}
		}

		public IEnumerable<PicasaConfigElement> PicasaConfigElements { get { return _picasa; } }

		private void LoadPicasaSettings(XContainer configDoc)
		{
			var query = from store in configDoc.Descendants("Storage")
			            from media in store.Elements("Media")
			            from picasa in media.Elements("Picasa")
			            select picasa.Elements();

			foreach(var item in query.SelectMany(item => item))
			{
				var label = (string)item.Attribute("label");
				if (String.IsNullOrEmpty(label)) continue;
				switch(item.Name.LocalName)
				{
					case "User":
						var user = (string)item.Attribute("user");
						if (String.IsNullOrEmpty(user)) continue;
						_picasa.Add(new PicasaUserConfigElement(label, user));
						break;
					case "Text":
						var queryText = (string)item.Attribute("query");
						if (String.IsNullOrEmpty(queryText)) continue;
						_picasa.Add(new PicasaTextConfigElement(label, queryText));
						break;
					case "Tags":
						var queryTags = (string)item.Attribute("query");
						if (String.IsNullOrEmpty(queryTags)) continue;
						_picasa.Add(new PicasaTagConfigElement(label, queryTags));
						break;
					case "Featured":
						_picasa.Add(new PicasaFeaturedConfigElement(label));
						break;
				}
			}
		}

		public IEnumerable<FlickrConfigElement> FlickrConfigElements { get { return _flickr; } }

		private void LoadFlickrSettings(XContainer configDoc)
		{
			var query = from store in configDoc.Descendants("Storage")
			            from media in store.Elements("Media")
			            from flickr in media.Elements("Flickr")
			            select flickr.Elements();

			foreach(var item in query.SelectMany(item => item))
			{
				var label = (string)item.Attribute("label");
				if (String.IsNullOrEmpty(label)) continue;
				switch(item.Name.LocalName)
				{
					case "User":
						var user = (string)item.Attribute("user");
						if (String.IsNullOrEmpty(user)) continue;
						_flickr.Add(new FlickrUserConfigElement(label, user));
						break;
					case "Interesting":
						_flickr.Add(new FlickrInterestingConfigElement(label));
						break;
					case "Location":
						var latitude = item.Attribute("latitude");
						var longitude = item.Attribute("longitude");
						var radius = item.Attribute("radius");
						if (latitude == null || longitude == null || radius == null) continue;
						_flickr.Add(new FlickrLocationConfigElement(label, 
						                                            (float)latitude,
						                                            (float)longitude,
						                                            (float)radius));
						break;
					case "Text":
						var textQuery = (string)item.Attribute("query");
						if (String.IsNullOrEmpty(textQuery)) continue;
						_flickr.Add(new FlickrTextConfigElement(label, textQuery));
						break;
					case "Tags":
						var tagQuery = (string)item.Attribute("query");
						if (String.IsNullOrEmpty(tagQuery)) continue;
						_flickr.Add(new FlickrTagsConfigElement(label, tagQuery));
						break;
				}
			}
		}

		
		private void LoadStaticResourceSettings(XContainer configDoc)
		{
			var resourceQuery = from item in configDoc.Descendants("Storage")
			                    let stat = item.Attribute("staticPath")
			                    where stat != null
			                    select (string)stat;

			var storage = resourceQuery.FirstOrDefault();
			StaticResources = String.IsNullOrEmpty(storage) ? "UPNP/Resources" : storage;
		}

		private void LoadPortSettings(XContainer configDoc)
		{
			var portQuery =
				from item in configDoc.Elements("Network")
				let portNode = item.Element("Port")
				where portNode != null && !String.IsNullOrEmpty((string) portNode) &&
				      Regex.IsMatch((string) portNode, "^[0-9]+$")
				select ((string)portNode).Trim();

			int port;
			Port = int.TryParse(portQuery.FirstOrDefault(), out port) ? port : 12345;
		}

		public string ServerName { get { return "MacOSX UPnP/1.0 GUPNPMS/0.1"; } }
		public string FriendlyName { get; private set; }
		public Guid DeviceId { get; private set; }
		public int Port { get; private set; }
		public string StaticResources { get; private set; }
		public string MovieIcon { get; private set; }
		public string ImageIcon { get; private set; }
		public string MusicIcon { get; private set; }
		public string ServerIcon { get; private set; }
		
		public IDictionary<string,string> iTunesFolders
		{
			get
			{
				return _itunesLibraries;
			}
		}
		public IDictionary<string,string> iPhotoFolders
		{
			get
			{
				return _iphotoLibraries;
			}
		}

		public IDictionary<string,string> MediaFolders
		{
			get
			{
				return _mediaFolders;
			}
		}
	}
}
