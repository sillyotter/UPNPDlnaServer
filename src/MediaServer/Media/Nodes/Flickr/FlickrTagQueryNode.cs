using System;
using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	class FlickrTagQueryNode : FlickrQueryNode
	{
		public FlickrTagQueryNode(FolderNode parentNode, string title, string tags)
			: base(parentNode, title, new PhotoSearchOptions { 
			                                                 	Page = 1, PerPage = 100, Tags = tags, TagMode = TagMode.AnyTag, 
			                                                 	MinTakenDate = DateTime.Now - TimeSpan.FromDays(7),
			                                                 	SortOrder = PhotoSearchSortOrder.DateTakenDesc})
		{
		}
	}
}