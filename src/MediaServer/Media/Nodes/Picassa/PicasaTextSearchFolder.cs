using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	class PicasaTextSearchFolder : PicasaSearchFolderBase
	{
		private readonly string _search;

		public PicasaTextSearchFolder(FolderNode parentNode, string title, string search) : base(parentNode, title)
		{
			_search = search;
		}

		#region Overrides of WebResourceFolderBase

		protected override void FetchImageInformation()
		{
			var query = new PhotoQuery("http://picasaweb.google.com/data/feed/api/all")
			            	{
			            		Query = _search,
			            	};

			AddPicturesToFolder(query);
		}

		#endregion
	}
}