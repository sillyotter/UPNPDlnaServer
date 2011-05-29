using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Xml.Linq;
using MediaServer.Configuration;

namespace MediaServer.Media.Nodes
{
	public class MusicNode : AvFileNode
	{
		private string _title;
		private string _artist;
		private string _album;
		private string _genre;
		private TagLib.IPicture _art;
		private bool _artcheck;

		public MusicNode(FolderNode parentNode, string title,  string location) 
			: base(parentNode, title, location)
		{
		}
		
		#region Overrides of MediaNode

		public override string Class
		{
			get { return UpnpTypeLookup.MusicType; }
		}

		#endregion

		#region Overrides of FileNode

	    public override Uri GetIconUrl(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
	    {
	        if (AlbumArt != null)
			{
				return new Uri(String.Format("http://{0}/MediaServer/GetMusicImage?id={1}", queryEndpoint, Id));
			}
	        return new Uri(String.Format("http://" + mediaEndpoint + "/MediaServer/" + Settings.Instance.MusicIcon));
	    }

	    #endregion
		
		public TagLib.IPicture AlbumArt 
		{
			get
			{
				if (_art == null && _artcheck == false)
				{
					var tagfile = TagLib.File.Create(Location);
					var img = tagfile.Tag.Pictures.FirstOrDefault();
					if (img != null)
					{
						_art = img;
					}
					_artcheck = true;
				}	
				return _art;
			}
		}
		public override string Title
		{
			get
			{
				if (_title == null)
				{
					string tag = null;
					try {
						var file = TagLib.File.Create(Location);
						tag = Path.GetFileNameWithoutExtension(file.Tag.Title);
					} catch (Exception) {} 
					_title = !String.IsNullOrEmpty(tag) ? tag : base.Title;
				}
				return _title;
			}
		}
		
		public string Artist 
		{ 
			get
			{
				if (_artist == null)
				{
					string tag = null;
					try {
						var file = TagLib.File.Create(Location);
						tag = file.Tag.FirstPerformer;
					}
					catch (Exception) { } 
					_artist = !String.IsNullOrEmpty(tag) ? tag : String.Empty;
				}
				return _artist;
			} 
		}
		
		public string Album 
		{ 
			get
			{
				if (_album == null)
				{
					string tag = null;
					try {
						var file = TagLib.File.Create(Location);
						tag = file.Tag.Album;
					}
					catch (Exception) { } 
					_album = !String.IsNullOrEmpty(tag) ? tag : String.Empty;
				}
				return _album;
			}
		}
		
		public string Genre 
		{ 
			get
			{
				if (_genre == null)
				{
					string tag = null;
					try {
						var file = TagLib.File.Create(Location);
						tag = file.Tag.FirstGenre;
					}
					catch (Exception) { } 
					_genre = !String.IsNullOrEmpty(tag) ? tag : String.Empty;
				}
				return _genre;
			}
		}
		
		public uint? SampleFrequencyHz { get; internal set; }
		
		public override XElement RenderMetadata(IPEndPoint queryEndpoint, IPEndPoint mediaEndpoint)
		{
			var results = base.RenderMetadata(queryEndpoint, mediaEndpoint);
			if (!String.IsNullOrEmpty(Artist)) results.Add(new XElement(Upnp + "artist", Artist));
			if (!String.IsNullOrEmpty(Album)) results.Add(new XElement(Upnp + "album", Album));
			if (!String.IsNullOrEmpty(Genre)) results.Add(new XElement(Upnp + "genre", Genre));
			
			var res = results.Element(Didl + "res");
			if (res != null)
			{
				if (SampleFrequencyHz.HasValue) res.Add(new XAttribute("sampleFrequency", SampleFrequencyHz));
			}
			
			return results;
		}
	}
}
