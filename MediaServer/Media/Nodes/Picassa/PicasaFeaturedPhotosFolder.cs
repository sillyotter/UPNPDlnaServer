using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	class PicasaFeaturedPhotosFolder : PicasaSearchFolderBase
	{
		public PicasaFeaturedPhotosFolder(FolderNode parentNode, string title) : base(parentNode, title)
		{
		}

		#region Overrides of WebResourceFolderBase

		protected override void FetchImageInformation()
		{
			var query = new PhotoQuery("http://picasaweb.google.com/data/feed/api/featured");
			AddPicturesToFolder(query);
		}

		#endregion
	}
}