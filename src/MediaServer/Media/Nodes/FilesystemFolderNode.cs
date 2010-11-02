using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;

namespace MediaServer.Media.Nodes
{
	public class FilesystemFolderNode : FolderNode
	{
		private readonly string _location;
		private volatile bool _hasBeenScanned;
		private readonly FileSystemWatcher _watcher;
		
		public FilesystemFolderNode(FolderNode parentNode, string title, string location) 
			: base(parentNode, title)
		{
			_location = location;

			_watcher = new FileSystemWatcher(_location, "*")
			           	{
			           		IncludeSubdirectories = false,
			           		EnableRaisingEvents = true,
			           	};

			_watcher.Created += OnCreated;
			_watcher.Deleted += OnDeleted;
		}

		private void OnDeleted(object sender, FileSystemEventArgs e)
		{
			_hasBeenScanned = false;
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			var path = e.FullPath;
			MediaNode node = null;

			if (Directory.Exists(path))
			{
				node = new FilesystemFolderNode(this, Path.GetFileName(path), path);
			}
			else if (File.Exists(path))
			{
				if (String.IsNullOrEmpty(UpnpTypeLookup.GetUpnpType(path))) return;
				node = FileNode.Create(this, path);
			}

			if (node != null)
			{
				Add(node);
			}
		}

		public void RemoveAllChildren()
		{
			foreach(var child in this)
			{
				child.RemoveFromIndexes();
			}

			Clear();
		}

		private void ScanFilesystem()
		{
			RemoveAllChildren();

			var dirs =
				from item in Directory.GetDirectories(_location)
				select new FilesystemFolderNode(this, Path.GetFileName(item), item);
			
			var mediaFiles =
				from item in Directory.GetFiles(_location)
				where UpnpTypeLookup.GetSupportedExtensions().Contains(Path.GetExtension(item))
				select FileNode.Create(this, item);

			AddRange(dirs.Cast<MediaNode>().Concat(mediaFiles.Cast<MediaNode>()).ToList());
		}

		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			lock(this)
			{
				if (!_hasBeenScanned)
				{
					_hasBeenScanned = true;
				    ScanFilesystem();
				}
				return base.RenderMetadata(queryEndpoint, mediaEndpoint);
			}
		}
	}
}
