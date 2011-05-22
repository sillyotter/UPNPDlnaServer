using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Serialization;
using MediaServer.Utility;

namespace MediaServer.Configuration
{
	public class NetworkConfiguration
	{
		public NetworkConfiguration() : this(12345, 54321)
		{
		}

		public NetworkConfiguration(int mediaPort, int queryPort)
		{
			MediaPort = mediaPort;
			QueryPort = queryPort;
		}

		[XmlAttribute("mediaPort")]
		public int MediaPort { get; private set; }

		[XmlAttribute("queryPort")]
		public int QueryPort { get; private set; }
	}

	abstract public class NamedPathMediaElememnt
	{
		public NamedPathMediaElememnt() : this("Unknown", "Unknown")
		{
		}

		public NamedPathMediaElememnt(string name, string path)
		{
			Name = name;
			Path = path;
		}

		[XmlAttribute("name")]
		public string Name { get; private set; }

		[XmlAttribute("path")]
		public string Path { get; private set; }
	}

	public class Directory : NamedPathMediaElememnt
	{
		public Directory() : base()
		{
		}

		public Directory(string name, string path) : base(name, path)
		{
		}

	}

	public class RemapConfiguration
	{
		public RemapConfiguration()
		{
			Source = null;
			Destination = null;
		}
		public RemapConfiguration(string source, string destination)
		{
			Source = source;
			Destination = destination;
		}
		
		[XmlAttribute("src")]
		public string Source { get; private set; }

		[XmlAttribute("dest")]
		public string Destination { get; private set; }
	}

	public class iTunes : NamedPathMediaElememnt
	{
		public iTunes() : base() 
		{
		}

		public iTunes(string name, string path) : base (name, path)
		{
		}

		public RemapConfiguration Remap { get; private set; }
	}

	public class iPhoto : NamedPathMediaElememnt
	{
		public iPhoto() : base()
		{
		}

		public iPhoto(string name, string path) : base (name, path)
		{
		}

		public RemapConfiguration Remap { get; private set; }
	}

	public class IconConfiguration
	{
		public IconConfiguration()
		{
			Movie = "MovieIcon.png";
			Music = "MusicIcon.png";
			Image = "ImageIcon.png";
			Server = "ServerIcon.png";
		}

		public IconConfiguration(string movie, string music, string image, string server)
		{
			Movie = movie;
			Music = music;
			Image = image;
			Server = server;
		}

		[XmlAttribute("movie")]
		public string Movie { get; private set; }

		[XmlAttribute("music")]
		public string Music { get; private set; }

		[XmlAttribute("image")]
		public string Image { get; private set; }

		[XmlAttribute("server")]
		public string Server { get; private set; }
	}

	public class Configuration
	{
		public Configuration() 
		{
			Network = new NetworkConfiguration();
			Icons = new IconConfiguration();
			Media = new List<NamedPathMediaElememnt>();
		}

		public NetworkConfiguration Network { get; private set; }
		public IconConfiguration Icons { get; private set; }

		[
			XmlElement(typeof(Directory), ElementName="Directory"),
			XmlElement(typeof(iTunes), ElementName="iTunes"),
			XmlElement(typeof(iPhoto), ElementName="iPhoto")
		]
		public List<NamedPathMediaElememnt> Media { get; private set; }
	}

	sealed class Settings
	{
		private static readonly XNamespace Ns = "urn:schemas-upnp-org:device-1-0";

		#region Singleton

		private static readonly Lazy<Settings> SingletonInstance = new Lazy<Settings>(() => new Settings());

		#endregion

		private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
		private Configuration _configuration = new Configuration();

		public event EventHandler<EventArgs> ConfigurationChanged;

		private Settings()
		{
		}

		public static Settings Instance
		{
			get
			{
				return SingletonInstance.Value;
			}
		}

		public void LoadConfigurationFile(string filename)
		{
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
			using (var fs = new FileStream(filename, FileMode.Open))
			{
				var serializer = new XmlSerializer(typeof(Configuration));
				_configuration = (Configuration)serializer.Deserialize(fs);
			}
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

		public string ServerName { get { return "MacOSX UPnP/1.0 GUPNPMS/0.1"; } }
		public string FriendlyName { get; private set; }
		public Guid DeviceId { get; private set; }
		public int QueryPort { get { return _configuration.Network.QueryPort;  } }
		public int MediaPort { get { return _configuration.Network.MediaPort;  } }
		public string StaticResources 
		{
	 		get
			{
				return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "MediaServer/Resources/");
			}
		}		

		public string MovieIcon { get { return _configuration.Icons.Movie; }}
		public string ImageIcon { get { return _configuration.Icons.Image; }}
		public string MusicIcon { get { return _configuration.Icons.Music; }}
		public string ServerIcon { get { return _configuration.Icons.Server; }}
		
		public IEnumerable<iTunes> iTunesFolders
		{
			get
			{
				return _configuration.Media.OfType<iTunes>();
			}
		}
		public IEnumerable<iPhoto> iPhotoFolders
		{
			get
			{
				return _configuration.Media.OfType<iPhoto>();
			}
		}

		public IEnumerable<Directory> MediaFolders
		{
			get
			{
				return _configuration.Media.OfType<Directory>();
			}
		}
	}
}
