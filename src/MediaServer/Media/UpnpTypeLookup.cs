using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace MediaServer.Media
{
	internal static class UpnpTypeLookup
	{
		public const string FolderType = "object.container.storageFolder";
		public const string ImageType = "object.item.imageItem.photo";
		public const string MusicType = "object.item.audioItem.musicTrack";
		public const string VideoType = "object.item.videoItem.movie";
		public const string PlaylistType = "object.item.playlistItem";

		private static readonly IDictionary<string, string> UpnpItemTypes =
			new Dictionary<string, string>
				{
					{".m3u", PlaylistType},
					// Picked based on the supported formats of the PS3
					// Images
					{".jpg", ImageType},
					{".jpeg", ImageType},
					{".gif", ImageType},
					{".tif", ImageType},
					{".tiff", ImageType},
					{".bmp", ImageType},
					{".png", ImageType},
					// Audio
					{".mp3", MusicType},
					{".wav", MusicType},
					{".wma", MusicType},
					{".m4a", MusicType},
					{".aac", MusicType},
					{".m4p", MusicType},
					{".m4b", MusicType},
					// Video
					{".mpg", VideoType},
					{".mpeg", VideoType},
					{".wmv", VideoType},
					{".m2ts", VideoType},
					{".mts", VideoType},
					{".avi", VideoType},
					{".mjpg", VideoType},
					{".mjpeg", VideoType},
					{".mp4", VideoType},
					{".m4v", VideoType},
					{".264", VideoType},
					{".avc", VideoType},
					{".avr", VideoType},
					{".div", VideoType},
					{".divx", VideoType},
					{".dvx", VideoType},
					{".m-jpeg", VideoType},
					{".m2p", VideoType},
					{".m2s", VideoType},
					{".m2t", VideoType},
					{".m2v", VideoType},
					{".mfp", VideoType},
					{".mp2v", VideoType},
					{".xvid", VideoType},
				};

		public static string GetUpnpType(string filename)
		{
			string result;
			var ext = Path.GetExtension(filename).ToLower();
			return UpnpItemTypes.TryGetValue(ext, out result) ? result : String.Empty;
		}

		public static IEnumerable<string> GetSupportedExtensions()
		{
			return UpnpItemTypes.Keys;
		}

		public static string GetUpnpTypeForMime(string mimetype)
		{
			var exts = MimeTypeLookup.GetExtensionsForType(mimetype);
			var query = from uitem in UpnpItemTypes 
			            join ext in exts on uitem.Key equals ext
			            select uitem.Value;
			return query.FirstOrDefault();
		}

		public static bool IsValidExtension(string extension)
		{
			return UpnpItemTypes.ContainsKey(extension);
		}


	}
}
