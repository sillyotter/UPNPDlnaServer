using System;
using System.Net;
using System.Xml.Linq;

namespace MediaServer.Media.Nodes
{
	public abstract class AvFileNode : FileNode
	{
		protected AvFileNode(FolderNode parentNode, string title, string location) 
			: base(parentNode, title, location)
		{		
		}
		
		public uint? Bitrate { get; set; }
		public TimeSpan? Duration { get; set; }

		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			var results = base.RenderMetadata(queryEndpoint, mediaEndpoint);
			
			var res = results.Element(Didl + "res");
			if (res != null)
			{
				if (Bitrate.HasValue) res.Add(new XAttribute("bitrate", Bitrate/8));
				if (Duration.HasValue) res.Add(new XAttribute("duration", 
				                                              String.Format("{0:00}:{1:00}:{2:00}", Duration.Value.Hours, 
				                                                            Duration.Value.Minutes, 
				                                                            Duration.Value.Seconds)));
			}

			return results;
		}	
		
	}
}
