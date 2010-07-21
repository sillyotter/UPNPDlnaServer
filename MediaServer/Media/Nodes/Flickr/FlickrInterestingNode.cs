using System;

namespace MediaServer.Media.Nodes.Flickr
{
	class FlickrInterestingNode : FlickrBaseNode
	{
		public FlickrInterestingNode(FolderNode parentNode, string title) : base(parentNode, title)
		{
		}

		#region Overrides of FlickrBaseNode

		protected override void FetchImageInformation()
		{

			var photos = Flickr.InterestingnessGetList(DateTime.Now - TimeSpan.FromDays(2));
			foreach (var photo in photos)
			{
				CreateWebProxyNodeForImage(photo, this);
			}
		}

		#endregion
	}
}