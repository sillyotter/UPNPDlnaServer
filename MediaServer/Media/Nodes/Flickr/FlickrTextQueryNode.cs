using System;
using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	class FlickrTextQueryNode : FlickrQueryNode
	{
		public FlickrTextQueryNode(FolderNode parentNode, string title, string query) 
			: base(parentNode, title, new PhotoSearchOptions{
			                                                	Page = 1, PerPage = 100, Text = query, 
			                                                	MinTakenDate = DateTime.Now - TimeSpan.FromDays(7),
			                                                	SortOrder = PhotoSearchSortOrder.DateTakenDesc})
		{
		}
	}
}