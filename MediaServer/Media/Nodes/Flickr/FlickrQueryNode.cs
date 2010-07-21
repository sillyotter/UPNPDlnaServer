using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	abstract class FlickrQueryNode : FlickrBaseNode
	{
		private readonly PhotoSearchOptions _options;

		protected FlickrQueryNode(FolderNode parentNode, string title, PhotoSearchOptions options) : base(parentNode, title)
		{
			_options = options;
		}

		protected override void FetchImageInformation()
		{
			var photos = Flickr.PhotosSearch(_options);
			foreach (var p in photos)
			{
				CreateWebProxyNodeForImage(p, this);
			}
		}
	}
}