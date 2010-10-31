using System.Collections.Generic;
using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	class FlickrUserNode : FlickrBaseNode
	{
		private readonly string _username;

		public FlickrUserNode(FolderNode parentNode, string title, string username) : base(parentNode, title)
		{
			_username = username;
		}

		protected override void FetchImageInformation()
		{
			var user = Flickr.PeopleFindByUsername(_username);
			var psets = Flickr.PhotosetsGetList(user.UserId);
			foreach (var ps in psets.PhotosetCollection)
			{
				var psfolder = new FolderNode(this, ps.Title);
				Add(psfolder);

				var allImages = new List<Photo>();
				var i = 1;
				do
				{
					try
					{
						var setpictures = Flickr.PhotosetsGetPhotos(ps.PhotosetId, i++, 100).PhotoCollection;
						allImages.AddRange(setpictures);
					}
					catch (FlickrApiException)
					{
						break;
					}
				} while (true);

				foreach (var photo in allImages)
				{
					CreateWebProxyNodeForImage(photo, psfolder);
				}
			}
		}

		
	}
}