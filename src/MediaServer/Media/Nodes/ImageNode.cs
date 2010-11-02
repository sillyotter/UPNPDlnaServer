using System.Net;
using System.Web;
using System.Xml.Linq;
using System;

namespace MediaServer.Media.Nodes
{
	public class ImageNode : FileNode
	{
		public ImageNode(FolderNode parentNode, string title, string location) 
			: base(parentNode, title, location)
		{
		}

		#region Overrides of MediaNode

		public override string Class
		{
			get { return UpnpTypeLookup.ImageType; }
		}
		
		#endregion

		#region Overrides of FileNode

	    public override Uri GetIconUrl(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
	    {
	        return new Uri(String.Format("http://{0}/{1}", mediaEndpoint, HttpUtility.UrlPathEncode(Thumbnail)));
	    }

	    #endregion
		
		public string Thumbnail { get; internal set; }
		public uint? Width { get; internal set; }
		public uint? Height { get; internal set; }
		public uint? ColorDepth { get; internal set; }
		
		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			var results = base.RenderMetadata(queryEndpoint, mediaEndpoint);
			var res = results.Element(Didl + "res");
			if (res != null)
			{
				if (Width.HasValue && Height.HasValue) res.Add(new XAttribute("resolution", Width + "x" + Height));
				if (ColorDepth.HasValue) res.Add(new XAttribute("colorDepth", ColorDepth));
			}
			
			return results;
		}
		
	}
}
