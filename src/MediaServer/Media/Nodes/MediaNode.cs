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
			if (parentNode == null)
			{
				Id = Guid.Empty;
			}
			else
			{
				// var count = Interlocked.Increment(ref _count);
				Id = Guid.NewGuid(); // new Guid(String.Format("00000000-0000-0000-0000-{0:000000000000}", count));
			}

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

		public abstract XElement RenderMetadata(IPEndPoint endpoint);
		public abstract IEnumerable<XElement> RenderDirectChildren(uint startingIndex, uint requestedCount, IPEndPoint endpoint);

		public virtual void RemoveFromIndexes()
		{
			MediaRepository.Instance.RemoveNodeFromIndexes(this);
		}

	}
}