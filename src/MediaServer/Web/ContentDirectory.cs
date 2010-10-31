using System;
using System.Net;
using System.Threading;
using MediaServer.Media;
using MediaServer.Utility;

namespace MediaServer.Web
{
	class ContentDirectory : BaseSoapHandler
	{
		[SoapAction("urn:schemas-upnp-org:service:ContentDirectory:1#GetSearchCapabilities")]
		public void GetSearchCapabilities(
			[SoapParameter("SearchCaps")] out string searchCaps)
		{
			searchCaps = String.Empty;
		}

		[SoapAction("urn:schemas-upnp-org:service:ContentDirectory:1#GetSortCapabilities")]
		public void GetSortCapabilities(
			[SoapParameter("SortCaps")] out string sortCaps)
		{
			sortCaps = String.Empty;
		}

		[SoapAction("urn:schemas-upnp-org:service:ContentDirectory:1#GetSystemUpdateID")]
		public void GetSystemUpdateId(
			[SoapParameter("Id")] out uint updateId)
		{
			updateId = MediaRepository.Instance.SystemUpdateId;
		}

		[SoapAction("urn:schemas-upnp-org:service:ContentDirectory:1#Browse")]
		public void Browse(
			[SoapParameter("ObjectID")] string objectId,
			[SoapParameter("BrowseFlag")] string browseFlag,
			[SoapParameter("Filter")] string filter,
			[SoapParameter("StartingIndex")] uint startingIndex,
			[SoapParameter("RequestedCount")] uint requestedCount,
			[SoapParameter("SortCriteria")] string sortCriteria,
			[SoapParameter("Result")] out string result,
			[SoapParameter("NumberReturned")] out uint numberReturned,
			[SoapParameter("TotalMatches")] out uint totalMatches,
			[SoapParameter("UpdateID")] out uint updateId)
		{
			var localData = Thread.GetNamedDataSlot("localEndPoint");
			var endpoint = Thread.GetData(localData) as IPEndPoint;

			switch (browseFlag)
			{
				case "BrowseMetadata":
					MediaRepository.Instance.BrowseMetadata(objectId, filter, startingIndex, requestedCount, sortCriteria, out result,
					                                        out numberReturned, out totalMatches, out updateId, endpoint);
					break;
				case "BrowseDirectChildren":
					MediaRepository.Instance.BrowseDirectChildren(objectId, filter, startingIndex, requestedCount, sortCriteria,
					                                              out result, out numberReturned, out totalMatches, out updateId, 
																  endpoint);
					break;
				default:
					result = "";
					numberReturned = 0;
					totalMatches = 0;
					updateId = 0;
					break;

			}
		}
	}
}
