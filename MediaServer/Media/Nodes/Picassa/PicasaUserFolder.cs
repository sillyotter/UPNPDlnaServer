using System;
using System.Linq;
using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	class PicasaUserFolder : PicasaFolderBase
	{
		private readonly string _userId;

		public PicasaUserFolder(FolderNode parentNode, string title, string userId) 
			: base(parentNode, title)
		{
			_userId = userId;
		}
		
		protected override void  FetchImageInformation()
		{
			var albumQuery = new AlbumQuery(PicasaQuery.CreatePicasaUri(_userId));
			var albumFeed = Service.Query(albumQuery);
			foreach(var album in albumFeed.Entries)
			{
				var albumAccessor = new AlbumAccessor((PicasaEntry)album);
				var albumFolder = new FolderNode(this, albumAccessor.AlbumTitle);
				Add(albumFolder);

				var photoQuery = new PhotoQuery(PicasaQuery.CreatePicasaUri(_userId, albumAccessor.Id)); 
				// {NumberToRetrieve = base.NumberToRetrieve, thumbnailsize="144c"};
				var photoFeed = Service.Query(photoQuery);

				var photos =
					from photo in photoFeed.Entries.OfType<PicasaEntry>()
					select new WebProxyNode(albumFolder,
					                        photo.Media.Title.Value,
					                        new Uri(photo.Media.Content.Url),
					                        new Uri(photo.Media.Thumbnails.OrderBy(
					                                	mediaThumbnail => int.Parse(mediaThumbnail.Height)).First().Url));
				
				albumFolder.AddRange(photos.Cast<MediaNode>().ToList());
			}
		}
	}
}