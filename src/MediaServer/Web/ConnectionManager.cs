using System;

namespace MediaServer.Web
{
	class ConnectionManager : BaseSoapHandler
	{
		[SoapAction("urn:schemas-upnp-org:service:ConnectionManager:1#GetProtocolInformation")]
		public void GetProtocolInformation(
			[SoapParameter("Source")] out string source,
			[SoapParameter("Sink")] out string sink)
		{
			throw new NotImplementedException();
		}

		[SoapAction("urn:schemas-upnp-org:service:ConnectionManager:1#PrepareForConnection")]
		public void PrepareForConnection(
			[SoapParameter("RemoteProtocolInfo")] string remoteProtocolInfo,
			[SoapParameter("PeerConnectionManager")] string peerConnectionManager,
			[SoapParameter("PeerConnectionID")] int peerConnectionId,
			[SoapParameter("Direction")] string direction,
			[SoapParameter("ConnectionID")] out int connectionId,
			[SoapParameter("AVTransportID")] out int avTransportId,
			[SoapParameter("ResID")] out int resId)
		{
			throw new NotImplementedException();
		}

		[SoapAction("urn:schemas-upnp-org:service:ConnectionManager:1#ConnectionComplete")]
		public void ConnectionComplete(
			[SoapParameter("ConnectionID")] int connectionId)
		{
			throw new NotImplementedException();
		}

		[SoapAction("urn:schemas-upnp-org:service:ConnectionManager:1#GetCurrentConnectionIDs")]
		public void GetCurrentConnectionIDs(
			[SoapParameter("ConnectionIDs")] out string connectionIds)
		{
			throw new NotImplementedException();
		}

		[SoapAction("urn:schemas-upnp-org:service:ConnectionManager:1#GetCurrentConnectionInfo")]
		public void GetCurrentConnectionInfo(
			[SoapParameter("ConnectionID")] int connectionId,
			[SoapParameter("ResID")] out int resId,
			[SoapParameter("AVTransportID")] out int avTransportId,
			[SoapParameter("ProtocolInfo")] out string protocolInfo,
			[SoapParameter("PeerConnectionManager")] out string peerConnectionManager,
			[SoapParameter("PeerConnectionID")] out int peerConnectionId,
			[SoapParameter("Direction")] out string direction,
			[SoapParameter("Status")] out string status)
		{
			throw new NotImplementedException();
		}
	}
}