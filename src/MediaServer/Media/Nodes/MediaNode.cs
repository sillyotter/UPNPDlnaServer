using System;
using System.Net;
using System.Xml.Linq;
using System.Collections.Generic;

namespace MediaServer.Media.Nodes
{
	public abstract class MediaNode
	{
		protected static readonly XNamespace Didl = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
		protected static readonly XNamespace Dc   = "http://purl.org/dc/elements/1.1/";
		protected static readonly XNamespace Upnp = "urn:schemas-upnp-org:metadata-1-0/upnp/";
		protected static readonly XNamespace Dlna = "urn:schemas-dlna-org:metadata-1-0/";
		
		//static long _count = 0;
		protected MediaNode(FolderNode parentNode)
		{
			Id = parentNode == null ? Guid.Empty : Guid.NewGuid();

			Parent = parentNode;
			MediaRepository.Instance.AddNodeToIndex(this);
		}

		public Guid Id { get; private set; }
		
		public Guid ParentId
		{ 
			get { return Parent != null ? Parent.Id : Guid.Empty; }
		}

		public FolderNode Parent { get; private set; }

		public abstract string Title { get; }
		public abstract string Class { get; }

		public abstract XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint);
		public abstract IEnumerable<XElement> RenderDirectChildren(uint startingIndex, uint requestedCount, IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint);

		public virtual void RemoveFromIndexes()
		{
			MediaRepository.Instance.RemoveNodeFromIndexes(this);
		}

	}
}
