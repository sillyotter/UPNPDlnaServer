using System;
using System.Linq;
using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	abstract class FlickrBaseNode : WebResourceFolderBase
	{
		private const string AppKey = "e62bfb8bb3c149b93ef4a794fcc1ad03";
		protected readonly FlickrNet.Flickr Flickr = new FlickrNet.Flickr(AppKey);

		protected FlickrBaseNode(FolderNode parentNode, string title)
			: base(parentNode, title)
		{
		}

		protected void CreateWebProxyNodeForImage(Photo photo, FolderNode parent)
		{
			var sizes = Flickr.PhotosGetSizes(photo.PhotoId).SizeCollection;
			var target = (from size in sizes
			              let area = size.Width*size.Height
			              orderby area descending
			              select new Uri(size.Source)).First();

			parent.Add(new WebProxyNode(parent, photo.Title, target, new Uri(photo.ThumbnailUrl)));
		}
	}
}