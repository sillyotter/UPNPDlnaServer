using System;
using System.Net;
using System.Xml.Linq;
using MediaServer.Configuration;

namespace MediaServer.Media.Nodes
{
	public class MovieNode : AvFileNode
	{
		public MovieNode(FolderNode parentNode, string title, string location)
			: base(parentNode, title, location)
		{
		}

		#region Overrides of MediaNode

		public override string Class
		{
			get { return UpnpTypeLookup.VideoType; }
		}

		#endregion

		#region Overrides of FileNode

	    public override Uri GetIconUrl(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
	    {
	        return new Uri(String.Format("http://{0}/MediaServer/" + Settings.Instance.MovieIcon, mediaEndpoint));
	    }

	    #endregion
		
		public uint? Width { get; internal set; }
		public uint? Height { get; internal set; }
		
		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			var results = base.RenderMetadata(queryEndpoint, mediaEndpoint);
			var res = results.Element(Didl + "res");
			if (res != null)
			{
				if (Width.HasValue && Height.HasValue) res.Add(new XAttribute("resolution", Width + "x" + Height));		
			}

			return results;
		}
	}
}
