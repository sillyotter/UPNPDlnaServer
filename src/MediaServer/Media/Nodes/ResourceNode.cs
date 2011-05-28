using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Xml.Linq;

namespace MediaServer.Media.Nodes
{
	public abstract class ResourceNode : MediaNode
	{
		protected ResourceNode(FolderNode parentNode) : base(parentNode)
		{
		}

	    public abstract Uri GetRequestUrl(IPEndPoint endpoint);
	    public abstract Uri GetIconUrl(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint);
	    public abstract string MimeType { get; }
		public abstract ulong Size { get; }

		#region Overrides of MediaNode

		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			return
				new XElement(
					Didl + "item",
					new XAttribute("id", Id),
					new XAttribute("parentID", ParentId),
					new XAttribute("childCount", 0),
					new XAttribute("restricted", true),
					new XElement(Dc + "title", new XText(HttpUtility.HtmlEncode(Title))),
					new XElement(Upnp + "class", new XText(Class)),
					new XElement(
						Upnp + "albumArtURI",
						new XAttribute(XNamespace.Xmlns + "dlna", Dlna.ToString()),
						new XAttribute(Dlna + "profileID", "PNG_TN"),
						new XText(HttpUtility.UrlPathEncode(GetIconUrl(queryEndpoint, mediaEndpoint).ToString()))),
					new XElement(
						Didl + "res",
						new XAttribute("protocolInfo", "http-get:*:" + MimeType + ":*"),
						new XAttribute("size", Size),
						new XText(HttpUtility.UrlPathEncode(GetRequestUrl(mediaEndpoint).ToString())))
					);
		}

		public override IEnumerable<XElement> RenderDirectChildren(uint startingIndex, uint requestedCount, IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			return new XElement[0];
		}

		#endregion
	}
}
