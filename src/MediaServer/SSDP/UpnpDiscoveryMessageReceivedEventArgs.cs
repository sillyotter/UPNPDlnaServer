using System;
using System.Net;
using  MediaServer.SSDP.Messages;

namespace MediaServer.SSDP
{
	public class UpnpDiscoveryMessageReceivedEventArgs : EventArgs
	{
		public IPEndPoint Source { get; private set; }
		public UpnpMessage Message { get; private set; }

		public UpnpDiscoveryMessageReceivedEventArgs(UpnpMessage message, IPEndPoint source)
		{
			Message = message;
			Source = source;
		}
	}
}
