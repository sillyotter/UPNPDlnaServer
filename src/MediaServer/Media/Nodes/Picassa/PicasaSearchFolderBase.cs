using System;
using System.Linq;
using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	abstract internal class PicasaSearchFolderBase : PicasaFolderBase
	{
		protected PicasaSearchFolderBase(FolderNode parentNode, string title) : base(parentNode, title)
		{
		}

		protected void AddPicturesToFolder(KindQuery query)
		{
			query.NumberToRetrieve = NumberToRetrieve;

			var feed = Service.Query(query);

			var photos =
				from photo in feed.Entries.OfType<PicasaEntry>()
				select new WebProxyNode(this,
				                        photo.Media.Title.Value,
				                        new Uri(photo.Media.Content.Url),
				                        new Uri(photo.Media.Thumbnails.OrderBy(
				                                	mediaThumbnail => int.Parse(mediaThumbnail.Height)).First().Url));

			AddRange(photos.Cast<MediaNode>().ToList());
		}
	}
}