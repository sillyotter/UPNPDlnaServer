using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using MediaServer.Configuration;
using MediaServer.Media.Nodes;
using MediaServer.Media.Nodes.Flickr;
using MediaServer.Media.Nodes.Picassa;
using MediaServer.Utility;
using System.Collections.Generic;

namespace MediaServer.Media
{
	public class MediaRepository : IMediaRepository
	{
		private static readonly XNamespace Didl = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
		private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
		private static readonly XNamespace Upnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";

		private readonly ReadWriteLockedCache<Guid, MediaNode> _resourceCache = 
			new ReadWriteLockedCache<Guid, MediaNode>();
		
		#region Singleton
		private static readonly MediaRepository SingletonInstance = new MediaRepository();
		public static MediaRepository Instance
		{
			get
			{
				return SingletonInstance;
			}
		}

		static MediaRepository() { }

		#endregion

		private MediaRepository()
		{ 
			//Settings.Instance.ConfigurationChanged += OnConfigurationChanged;
		}

		private void OnConfigurationChanged(object sender, EventArgs e)
		{
			Logger.Instance.Info("Configuration file changed, reloading media tree structure");
			Initialize();
		}

		private static bool AddMediaFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.MediaFolders
			             where Directory.Exists(item.Value)
			             select new FilesystemFolderNode(root, item.Key, item.Value)).Cast<MediaNode>().ToList();
			
			if (query.Count() > 0)
			{
				root.AddRange(query);
				return true;
			}
			return false;
		}

		private static bool AddiTunesFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.iTunesFolders
			             where File.Exists(item.Value)
			             select new iTunesFolderNode(root, item.Key, item.Value)).Cast<MediaNode>().ToList();

			var count = query.Count();
			if (count > 0)
			{
				root.AddRange(query);
				return true;
			}
			return false;

		}

		private static bool AddiPhotoFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.iPhotoFolders
			             where File.Exists(item.Value)
			             select new iPhotoFolderNode(root, item.Key, item.Value)).Cast<MediaNode>().ToList();

			var count = query.Count();
			if (count > 0)
			{
				root.AddRange(query);
				return true;
			}
			return false;
		}

		private static void AddFlickrFolders(FolderNode root)
		{
			var configElements = Settings.Instance.FlickrConfigElements;
			if (configElements.Count() <= 0) return;
			
			var flickrFolder = new FolderNode(root, "Flickr");
			root.Add(flickrFolder);

			flickrFolder.AddRange(
				configElements.OfType<FlickrUserConfigElement>().Select(
					item => new FlickrUserNode(flickrFolder, item.Label, item.User)).Cast<MediaNode>().ToList());

			flickrFolder.AddRange(
				configElements.OfType<FlickrInterestingConfigElement>().Select(
					item => new FlickrInterestingNode(flickrFolder, item.Label)).Cast<MediaNode>().ToList());

			flickrFolder.AddRange(
				configElements.OfType<FlickrLocationConfigElement>().Select(
					item => new FlickrLocationNode(flickrFolder, item.Label, item.Latitude, item.Longitude, item.Radius)).Cast<MediaNode>().ToList());

			flickrFolder.AddRange(
				configElements.OfType<FlickrTextConfigElement>().Select(
					item => new FlickrTextQueryNode(flickrFolder, item.Label, item.Text)).Cast<MediaNode>().ToList());

			flickrFolder.AddRange(
				configElements.OfType<FlickrTagsConfigElement>().Select(
					item => new FlickrTagQueryNode(flickrFolder, item.Label, item.Tags)).Cast<MediaNode>().ToList());
		}

		private static void AddPicasaFolder(FolderNode root)
		{
			var configElements = Settings.Instance.PicasaConfigElements;
			if (configElements.Count() <= 0) return;
			
			var picasaFolder = new FolderNode(root, "Picasa");
			root.Add(picasaFolder);

			picasaFolder.AddRange(configElements.OfType<PicasaUserConfigElement>().Select(
			                      	item => new PicasaUserFolder(picasaFolder, item.Label, item.UserId)).Cast<MediaNode>().ToList());

			picasaFolder.AddRange(configElements.OfType<PicasaTextConfigElement>().Select(
			                      	item => new PicasaTextSearchFolder(picasaFolder, item.Label, item.Query)).Cast<MediaNode>().ToList());

			picasaFolder.AddRange(configElements.OfType<PicasaTagConfigElement>().Select(
			                      	item => new PicasaTagSearchFolder(picasaFolder, item.Label, item.Query)).Cast<MediaNode>().ToList());

			picasaFolder.AddRange(configElements.OfType<PicasaFeaturedConfigElement>().Select(
			                      	item => new PicasaFeaturedPhotosFolder(picasaFolder, item.Label)).Cast<MediaNode>().ToList());
		}

		private void InternalInitialize(object state)
		{
			_resourceCache.Clear();

			var root = new FolderNode(null, "Root");
			
			AddMediaFolders(root);
			AddiTunesFolders(root);
			AddiPhotoFolders(root);
			
			var onlineFolder = new FolderNode(root, "Online");
			root.Add(onlineFolder);

			AddFlickrFolders(onlineFolder);
			AddPicasaFolder(onlineFolder);

			// This makes no sense.  how is this possible?  I guess mf. itf, or pf will be null if /Volumesn isnt up yet
			// so I guess this should stay...  got to think about it...
			// Actually, maybe not.
			//if (mf == false && itf == false && ipf == false) _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
		}

		public void Initialize()
		{
			InternalInitialize(null);
			//_timer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(-1));
		}

		public void AddNodeToIndex(MediaNode node)
		{
			_resourceCache.Add(node.Id, node);
		}

		public void AddNodesToIndex(IEnumerable<MediaNode> nodes)
		{
			_resourceCache.AddMulti(nodes.Select(item => new KeyValuePair<Guid, MediaNode>(item.Id, item)));
		}

		public void RemoveNodesFromIndexes(IEnumerable<MediaNode> nodes)
		{
			_resourceCache.DeleteMulti(nodes.Select(item => item.Id));
		}

		public void RemoveNodeFromIndexes(MediaNode node)
		{
			_resourceCache.Delete(node.Id);
		}
	
		#region Implementation of IMediaRepository

		public uint SystemUpdateId
		{
			get
			{
				return (uint)_resourceCache.Sum(item => item.GetHashCode());
			}
		}

		public void BrowseMetadata(string objectId, string filter, uint startingIndex,
			uint requestedCount, string sortCriteria, out string result, out uint numberReturned,
			out uint totalMatches, out uint updateId, IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			result = "";
			numberReturned = 0;
			totalMatches = 0;
			updateId = 0;

			var oid = (objectId == "0") ? Guid.Empty : new Guid(objectId);

			MediaNode node;
			if (_resourceCache.TryGetValue(oid, out node))
			{
				var folder = node as FolderNode;
				if (folder == null) return;

				var findings =
					new XElement(
						Didl + "DIDL-Lite",
						new XAttribute(XNamespace.Xmlns + "dc", Dc.ToString()),
						new XAttribute(XNamespace.Xmlns + "upnp", Upnp.ToString()),
						folder.RenderMetadata(queryEndpoint, mediaEndpoint));

				result = findings.ToString();
			
				numberReturned = 1;
				totalMatches = 1;
				updateId = oid == Guid.Empty ? SystemUpdateId : folder.ContainerUpdateId;
			}
		}

		public void BrowseDirectChildren(string objectId, string filter, uint startingIndex, 
			uint requestedCount, string sortCriteria, out string result, out uint numberReturned, 
			out uint totalMatches, out uint updateId, IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			result = "";
			numberReturned = 0;
			totalMatches = 0;
			updateId = 0;

			var oid = (objectId == "0") ? Guid.Empty : new Guid(objectId);

			if (requestedCount == 0) requestedCount = int.MaxValue;

			MediaNode node;
			if (_resourceCache.TryGetValue(oid, out node))
			{
				var folder = node as FolderNode;
				if (folder == null) return;
				var findings =
					new XElement(
						Didl + "DIDL-Lite",
						new XAttribute(XNamespace.Xmlns + "dc", Dc.ToString()),
						new XAttribute(XNamespace.Xmlns + "upnp", Upnp.ToString()),
						folder.RenderDirectChildren(startingIndex, requestedCount, queryEndpoint, mediaEndpoint)
						);

				result = findings.ToString();

				numberReturned = (uint) findings.Elements().Count();
				totalMatches = (uint)folder.Count;
				updateId = oid == Guid.Empty ? SystemUpdateId : folder.ContainerUpdateId;
			}
		}

		public ResourceNode GetNodeForId(Guid id)
		{
			MediaNode node;
			_resourceCache.TryGetValue(id, out node);
			return node as ResourceNode;
		}

		#endregion

	}
}
