using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using MediaServer.Utility;

namespace MediaServer.Media.Nodes
{
	public class iPhotoFolderNode : FolderNode
	{
		private readonly string _source;
		private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
		
		public iPhotoFolderNode(FolderNode parentNode, string title, string source) 
			: base(parentNode, title)
		{
			_source = source;
			
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
			var parseCommand = new Action<string>(ParseiPhotoFile);
			parseCommand.BeginInvoke(path, parseCommand.EndInvoke, null);
		}
				
		private void OnChanged(object sender, FileSystemEventArgs args)
		{
			_watcher.EnableRaisingEvents = false;
			
			foreach(var node in this)
			{
				node.RemoveFromIndexes();
			}
			Clear();

			ProcessFileInBackground(_source);
		}
		
		private static object MergeNodes(XElement node, IDictionary<string,XElement> data)
		{
			if (node.Name == "array")
			{
				return 
					from id in node.Elements("string")
					let sid = (string)id
					where data.ContainsKey(sid)
					select data[sid];
			}
			return (string)node;
		}

		private void ParseiPhotoFile(string location)
		{
			//Logger.Instance.Debug("parsing iphoto");
			try
			{
				if (!File.Exists(location)) return;
			
				var doc = XElement.Load(location);

				var root = doc.Element("dict");
				if (root == null) return;

				var images =
					from image in root.Elements("dict").Skip(2).Take(1).Elements("key")
					select new XElement(
						"Image",
						new XAttribute("id", (string) image),
						from key in image.ElementsAfterSelf("dict").First().Elements("key")
						let name = ((string) key).Replace(" ", "")
						let value = (string) (XElement) key.NextNode
						where name == "ImagePath" || name == "ThumbPath" || name == "Caption" || name == "Comment"
						select new XElement(name, value));	
				
				var viewableImages = 
					from image in images
					let ext = Path.GetExtension((string)image.Element("ImagePath")).ToLower()
					where ext != ".tiff" && ext != ".tif"
					select image;

				var storage = new Dictionary<string,XElement>();	
				
				foreach(var item in viewableImages)
				{
					var id = (string) item.Attribute("id");
					if (!String.IsNullOrEmpty(id))
					{
						storage[id] = item;
					}
				}
			
				var albumGroupFolder = new FolderNode(this, "Albums");
				var eventGroupFolder = new FolderNode(this, "Events");

				Add(albumGroupFolder);
				Add(eventGroupFolder);
			
				var secondRoot = root.Element("array");
				if (secondRoot == null) return;

			    var albums =
			        from album in secondRoot.Elements("dict")
			        select new XElement(
			            "Album",
			            from key in album.Elements("key")
			            let name = ((string) key).Replace(" ", "")
			            let nextnode = (XElement) key.NextNode
			            where name == "AlbumName" || name == "KeyList"
			            select new XElement(name, MergeNodes(nextnode, storage)));

				var nonEmptyAlbums = 
					from album in albums
					where album.Descendants("Image").Count() > 0
					select album;

				foreach(var item in nonEmptyAlbums)
				{
					var albumFolder = new FolderNode(albumGroupFolder, (string)item.Element("AlbumName"));
					albumGroupFolder.Add(albumFolder);
					AddImagesToFolder(albumFolder, item.Descendants("Image"));
				}

			    var events =
			        from e in root.Elements("array").Skip(1).Take(1).Elements("dict")
			        select new XElement(
			            "Event",
			            from key in e.Elements("key")
			            let name = ((string) key).Replace(" ", "")
			            let nextnode = (XElement) key.NextNode
			            where name == "RollName" || name == "KeyList"
			            select new XElement(name, MergeNodes(nextnode, storage)));
					
				var nonEmptyEvents =
					from ev in events
					where ev.Descendants("Image").Count() > 0
					select ev;

				foreach(var item in nonEmptyEvents)
				{
					var eventFolder = new FolderNode(eventGroupFolder, (string)item.Element("RollName"));
					eventGroupFolder.Add(eventFolder);
					var imageList = item.Descendants("Image");
					AddImagesToFolder(eventFolder, imageList);
				}
			}
			catch(Exception ex)
			{
				Logger.Instance.Exception("Error parsing iphoto xml", ex);
				throw;
			}
			finally
			{
				//Logger.Instance.Debug("done parsing iphoto");
				_watcher.EnableRaisingEvents = true;
			}
		}
		
		private static void AddImagesToFolder(FolderNode folder, IEnumerable<XElement> imageList)
		{
			foreach(var image in imageList)
			{
				var imagePath = (string)image.Element("ImagePath");
				if (String.IsNullOrEmpty(UpnpTypeLookup.GetUpnpType(imagePath))) continue;

				var title = (string)image.Element("Caption");
				var comment = (string)image.Element("Comment");
				var titleStr = !String.IsNullOrEmpty(comment) ? comment : title;

				var file = FileNode.Create(folder, titleStr, imagePath);
				
				folder.Add(file);

				var imageFile = file as ImageNode;
				if (imageFile != null)
				{
					imageFile.Thumbnail = (string)image.Element("ThumbPath");
				}
			}
		}
	}
}
