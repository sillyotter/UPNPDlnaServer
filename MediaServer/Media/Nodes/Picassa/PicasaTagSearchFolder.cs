using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	class PicasaTagSearchFolder : PicasaSearchFolderBase
	{
		private readonly string _tagSearch;

		public PicasaTagSearchFolder(FolderNode parentNode, string title, string tags) : base(parentNode, title)
		{
			_tagSearch = tags;
		}

		#region Overrides of WebResourceFolderBase

		protected override void FetchImageInformation()
		{
			var query = new PhotoQuery("http://picasaweb.google.com/data/feed/api/all") 
			            	{ 
			            		Tags = _tagSearch, 
			            	};

			AddPicturesToFolder(query);
		}

		#endregion
	}
}