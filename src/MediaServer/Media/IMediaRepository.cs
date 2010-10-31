using System;
using System.Net;
using MediaServer.Media.Nodes;

namespace MediaServer.Media
{
	public interface IMediaRepository
	{
		uint SystemUpdateId { get; }

		void BrowseMetadata(
            string objectId, 
            string filter, 
            uint startingIndex, 
            uint requestedCount, 
            string sortCriteria, 
            out string result, 
            out uint numberReturned, 
            out uint totalMatches, 
            out uint updateId, 
            IPEndPoint endpoint);

	    void BrowseDirectChildren(
	        string objectId,
	        string filter,
	        uint startingIndex,
	        uint requestedCount,
	        string sortCriteria,
	        out string result,
	        out uint numberReturned,
	        out uint totalMatches,
	        out uint updateId,
	        IPEndPoint endpoint);

		ResourceNode GetNodeForId(Guid id);
	}
}