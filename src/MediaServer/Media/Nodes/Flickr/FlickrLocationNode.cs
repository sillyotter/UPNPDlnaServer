using System;
using FlickrNet;

namespace MediaServer.Media.Nodes.Flickr
{
	class FlickrLocationNode : FlickrQueryNode
	{
		public FlickrLocationNode(FolderNode parentNode, string title, float lat, float lon, float radius) 
			: base(parentNode, title, new PhotoSearchOptions {
			                                                 	MinTakenDate = DateTime.Now - TimeSpan.FromDays(7),
			                                                 	SortOrder = PhotoSearchSortOrder.InterestingnessDesc, 
			                                                 	Latitude =  lat, Longitude =  lon, Radius = radius, RadiusUnits = RadiusUnits.Miles, 
			                                                 	Page = 1, PerPage = 100 })
		{
		}
	}
}