using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using MediaServer.Utility;

namespace MediaServer.Media.Nodes
{
	public class iTunesFolderNode : FolderNode
	{
		private readonly string _source;
		private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
		private readonly Configuration.RemapConfiguration _remap;
		
		public iTunesFolderNode(FolderNode parentNode, string title, string source, Configuration.RemapConfiguration remap) 
			: base(parentNode, title)
		{
			_source = source;
			_remap = remap;
			
			//_watcher.IncludeSubdirectories = false;
			//_watcher.Path = Path.GetDirectoryName(_source);
			//_watcher.Filter = Path.GetFileName(_source);
			//_watcher.EnableRaisingEvents = false;
			//_watcher.NotifyFilter = NotifyFilters.LastWrite;
			//_watcher.Changed += OnChanged;		
			
			ProcessFileInBackground(_source);
		}
		
		private void ProcessFileInBackground(string path)
		{
			var parseCommand = new Action<string>(ParseiTunesFile);
			parseCommand.BeginInvoke(path, parseCommand.EndInvoke, null);
		}
		
		//private void OnChanged(object sender, FileSystemEventArgs args)
		//{
		//    _watcher.EnableRaisingEvents = false;
		//    foreach(var item in this)
		//    {
		//        item.RemoveFromIndexes();
		//    }
		//    Clear();
		//    ProcessFileInBackground(_source);
		//}
		
		
		private static IEnumerable<IGrouping<string,XElement>> GetGroupings(XContainer source, string type)
		{
		    var query =
		        from item in source.Descendants("Track")
		        let elem = item.Element(type)
		        where elem != null
		        group item by elem.Value
		        into g
		            orderby g.Key
		            select g;

			return query;

		}

		private static object MergeTrees(string value, IDictionary<string,XElement> songs)
		{
			if (!value.StartsWith("Track ID"))
			{
				return value;
			}
			return value.Replace("Track ID", " ").Trim().Split(' ')
				.Where(songs.ContainsKey)
				.Select(item => songs[item])
				.OrderBy(item => (string)item.Element("Album"))
				.ThenBy(item => (string)item.Element("TrackNumber"));
		}

		private void AddSubFolder(FolderNode root, XContainer source, string type)
		{
			var name = type + "s";
			var newFolder = new FolderNode(root, name);
			root.Add(newFolder);
			
			var group = GetGroupings(source, type);
			foreach(var item in group)
			{	
				AddMedia(newFolder, item.Key, item);
			}
		}
		
		private void AddMedia(FolderNode root, string newFolderName, IEnumerable<XElement> source)
		{
			var newFolder = new FolderNode(root, newFolderName);
			root.Add(newFolder);
			
			foreach(var track in source)
			{
				var loc = (string)track.Element("Location");
				if (!String.IsNullOrEmpty(loc))
				{
					var uri = new Uri(loc);
					var path = uri.LocalPath;
					if (String.IsNullOrEmpty(UpnpTypeLookup.GetUpnpType(path))) continue;

					var title = (string)track.Element("Name");

					var file = !String.IsNullOrEmpty(title) ? FileNode.Create(newFolder, title, path.Replace(_remap.Source, _remap.Destination)) : 
						FileNode.Create(newFolder, Path.GetFileNameWithoutExtension(path), path.Replace(_remap.Source, _remap.Destination));

					newFolder.Add(file);

					var avfile = file as AvFileNode;
					if (avfile != null)
					{
						var dur = (string)track.Element("TotalTime");
						if (!String.IsNullOrEmpty(dur))
						{
							avfile.Duration = TimeSpan.FromMilliseconds(int.Parse(dur));
						}
						var br = (string)track.Element("BitRate");
						if (!String.IsNullOrEmpty(br))
						{
							avfile.Bitrate = uint.Parse(br) * 1000;
						}
					}
					
					var mfile = file as MusicNode;
					if (mfile != null)
					{
						var sr = (string)track.Element("SampleRate");
						if (!String.IsNullOrEmpty(sr))
						{
							mfile.SampleFrequencyHz = uint.Parse(sr);
						}
					}

					var vfile = file as MovieNode;
					if (vfile != null)
					{
						
						var w = (string)track.Element("VideoWidth");
						if (!String.IsNullOrEmpty(w))
						{
							vfile.Width = uint.Parse(w);
						}
						var h = (string)track.Element("VideoHeight");
						if (!String.IsNullOrEmpty(h))
						{
							vfile.Height = uint.Parse(h);
						}	
						
					}
				}
			}
		}
			
		private void ParseiTunesFile(string location)
		{
			//Logger.Instance.Debug("parsing itunes");
			try	
			{
				if (!File.Exists(location)) return;
			
				var doc = XElement.Load(location);
				var songs = new Dictionary<string,XElement>();

				var root = doc.Element("dict");
				if (root == null) return;
				var nextroot = root.Element("dict");
				if (nextroot == null) return;

			    var rawTracks =
			        from track in nextroot.Elements("dict")
			        select new XElement(
			            "Track",
			            from key in track.Elements("key")
			            let name = ((string) key).Replace(" ", "")
			            let value = (string) (XElement) key.NextNode
			            select new XElement(name, value));

				var useableElements = 
					from item in rawTracks 
					where (string)item.Element("Kind") != "MPEG audio stream" && 
					      item.Element("Protected") == null
					select item;
			
				foreach(var item in useableElements)
				{
					var id = (string)item.Element("TrackID");
					if (!String.IsNullOrEmpty(id))
					{
						songs[id] = item;
					}
				}

				var thirdroot = root.Element("array");
				if (thirdroot == null) return;

				var rawPlaylists =
					(from playlist in thirdroot.Elements("dict")
					 select new XElement(
					 	"Playlist",
					 	from key in playlist.Elements("key")
					 	let name = ((string) key).Replace(" ", "")
					 	let nextnode = (XElement) key.NextNode
					 	where name == "Name" || name == "PlaylistItems"
					 	select new XElement(name, MergeTrees((string) nextnode, songs)))
					).ToList();
			
				var musicPlaylist = 
					(from item in rawPlaylists
					 let name = (string)item.Element("Name")
					 where name == "Music"
					 select item).First();
			
				var musicFolder = new FolderNode(this, "Music");
				Add(musicFolder);
				
				AddSubFolder(musicFolder, musicPlaylist, "Artist");
				AddSubFolder(musicFolder, musicPlaylist, "Album");
				AddSubFolder(musicFolder, musicPlaylist, "Composer");
				AddSubFolder(musicFolder, musicPlaylist, "Genre");
			
				AddMedia(musicFolder, "Songs", musicPlaylist.Descendants("Track"));
			
				var playlistFolder = new FolderNode(musicFolder, "Playlists");
				Add(playlistFolder);

				var ignoredPlaylists = new []{"Music", "Library", "Audiobooks", "Genius", "iTunes DJ", "Purchased"};
				var filteredPlaylists = 
					from item in rawPlaylists 
					let name = (string)item.Element("Name")
					where !String.IsNullOrEmpty(name) && !ignoredPlaylists.Contains(name)
					orderby name
					select item;
				
				foreach(var item in filteredPlaylists)
				{
					var name = (string)item.Element("Name");
					var tracks = item.Descendants("Track");
			
					switch(name)
					{
						case "Movies":
						case "TV Shows":
						case "Podcasts":
						case "iTunes U":
							AddMedia(this, name, tracks);
							break;
						default:
							AddMedia(playlistFolder, name, tracks);
							break;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Instance.Exception("Error parsing itunes xml", ex);
				throw;
			}
			finally
			{
				_watcher.EnableRaisingEvents = true;
				//Logger.Instance.Debug("done parsing itunes");
			}
		}
	}
}
