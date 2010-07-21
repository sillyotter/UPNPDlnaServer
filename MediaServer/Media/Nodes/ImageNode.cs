using System.Net;
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

	    public override Uri GetIconUrl(IPEndPoint baseAddr)
	    {
	        return new Uri(String.Format("http://{0}/MediaServer/GetImageThumbnail?id={1}", baseAddr, Id));
	    }

	    #endregion
		
		public string Thumbnail { get; internal set; }
		public uint? Width { get; internal set; }
		public uint? Height { get; internal set; }
		public uint? ColorDepth { get; internal set; }
		
		public override XElement RenderMetadata(IPEndPoint endpoint)
		{
			var results = base.RenderMetadata(endpoint);
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
