using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;

namespace MediaServer.Media.Nodes
{
	public abstract class ResourceNode : MediaNode
	{
		protected ResourceNode(FolderNode parentNode) : base(parentNode)
		{
		}

	    public abstract Uri GetRequestUrl(IPEndPoint baseAddress);
	    public abstract Uri GetIconUrl(IPEndPoint baseAddr);
	    public abstract string MimeType { get; }
		public abstract ulong Size { get; }

		#region Overrides of MediaNode

		public override XElement RenderMetadata(IPEndPoint endpoint)
		{
			return
				new XElement(
					Didl + "item",
					new XAttribute("id", Id),
					new XAttribute("parentID", ParentId),
					new XAttribute("childCount", 0),
					new XAttribute("restricted", true),
					new XElement(Dc + "title", new XText(Title)),
					new XElement(Upnp + "class", new XText(Class)),
					new XElement(
						Upnp + "albumArtURI",
						new XAttribute(XNamespace.Xmlns + "dlna", Dlna.ToString()),
						new XAttribute(Dlna + "profileID", "PNG_TN"),
						new XText(GetIconUrl(endpoint).ToString())),
					new XElement(
						Didl + "res",
						new XAttribute("protocolInfo", "http-get:*:" + MimeType + ":*"),
						new XAttribute("size", Size),
						new XText(GetRequestUrl(endpoint).ToString()))
					);
		}

		public override IEnumerable<XElement> RenderDirectChildren(uint startingIndex, uint requestedCount, IPEndPoint endpoint)
		{
			return new XElement[0];
		}

		#endregion
	}
}
