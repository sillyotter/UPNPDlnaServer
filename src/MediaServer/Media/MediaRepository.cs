using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using MediaServer.Configuration;
using MediaServer.Media.Nodes;
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
		private static readonly Lazy<MediaRepository> SingletonInstance = new Lazy<MediaRepository>(() => new MediaRepository());
		public static MediaRepository Instance
		{
			get
			{
				return SingletonInstance.Value;
			}
		}

		#endregion

		private MediaRepository()
		{ 
			//Settings.Instance.ConfigurationChanged += OnConfigurationChanged;
		}

		//private void OnConfigurationChanged(object sender, EventArgs e)
		//{
		//    Logger.Instance.Info("Configuration file changed, reloading media tree structure");
		//    Initialize();
		//}

		private static void AddMediaFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.MediaFolders
			             where System.IO.Directory.Exists(item.Path)
			             select new FilesystemFolderNode(root, item.Name, item.Path)).Cast<MediaNode>().ToList();
			
			if (query.Count() > 0)
			{
				root.AddRange(query);
			}
		}

		private static void AddiTunesFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.iTunesFolders
			             where File.Exists(item.Path)
			             select new iTunesFolderNode(root, item.Name, item.Path, item.Remap)).Cast<MediaNode>().ToList();

			var count = query.Count();
			if (count > 0)
			{
				root.AddRange(query);
			}
		}

		private static void AddiPhotoFolders(FolderNode root)
		{
			var query = (from item in Settings.Instance.iPhotoFolders
			             where File.Exists(item.Path)
			             select new iPhotoFolderNode(root, item.Name, item.Path, item.Remap)).Cast<MediaNode>().ToList();

			var count = query.Count();
			if (count > 0)
			{
				root.AddRange(query);
			}
		}

		private void InternalInitialize()
		{
			_resourceCache.Clear();

			var root = new FolderNode(null, "Root");
			
			AddMediaFolders(root);
			AddiTunesFolders(root);
			AddiPhotoFolders(root);
			
			//var onlineFolder = new FolderNode(root, "Online");
			//root.Add(onlineFolder);

			// This makes no sense.  how is this possible?  I guess mf. itf, or pf will be null if /Volumesn isnt up yet
			// so I guess this should stay...  got to think about it...
			// Actually, maybe not.
			//if (mf == false && itf == false && ipf == false) _timer.Change(TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(-1));
		}

		public void Initialize()
		{
			InternalInitialize();
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
				unchecked
				{
					return _resourceCache.VersionId;
				}
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
