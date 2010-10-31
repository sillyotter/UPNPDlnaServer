using System;
using System.IO;
using System.Net;
using System.Xml.Linq;

namespace MediaServer.Media.Nodes
{
	public abstract class FileNode : ResourceNode
	{
		private readonly string _title;
		private readonly string _location;
		private ulong _size;

		public static FileNode Create(FolderNode parent, string location)
		{
			return Create(parent, Path.GetFileNameWithoutExtension(location), location);
		}

		public static FileNode Create(FolderNode parent, string title, string location)
		{
			var upnpType = UpnpTypeLookup.GetUpnpType(location);
			FileNode node;
			switch (upnpType)
			{
				case UpnpTypeLookup.ImageType:
					node = new ImageNode(parent, title, location);
					break;
				case UpnpTypeLookup.MusicType:
					node = new MusicNode(parent, title, location);
					break;
				case UpnpTypeLookup.VideoType:
					node = new MovieNode(parent, title, location);
					break;
				default:
					return null;
			}
			return node;
		}

		protected FileNode(FolderNode parentNode, string title, string location) 
			: base(parentNode)
		{
			_title = title;
			_location = location;
		}

		public string Location
		{
			get
			{
				return _location;
			}
		}

		public override string Title 
		{ 
			get 
			{ 
				return _title ?? Path.GetFileNameWithoutExtension(_location); 
			}
		}

		public override ulong Size 
		{
			get
			{
				if (_size == 0)
				{
					var fi = new FileInfo(_location);
					_size = (ulong)fi.Length;
				}
				return _size;
			}
		}

		public override string MimeType 
		{
			get
			{
				return MimeTypeLookup.GetMimeType(_location);
			}
		}
		
		public DateTime ModifiedTime
		{
			get
			{
				return File.GetLastWriteTime(_location);
			}
		}

	    public override Uri GetRequestUrl(IPEndPoint baseAddress)
	    {
	        return new Uri(string.Format("http://{0}/MediaServer/GetMedia?id={1}", baseAddress, Id));
	    }

	    public override XElement RenderMetadata(IPEndPoint endpoint)
		{
			var results = base.RenderMetadata(endpoint);
			if (results != null)
			{
				results.Add(new XElement(Dc + "date", ModifiedTime.ToString("s")));
			}
			return results;
		}
		
	}
}
