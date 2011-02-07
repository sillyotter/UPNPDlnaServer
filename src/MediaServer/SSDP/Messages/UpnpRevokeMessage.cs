using System;

namespace MediaServer.SSDP.Messages
{
	public class UpnpRevokeMessage : UpnpAdvertiseMessage
	{
		public UpnpRevokeMessage(string data)
			: base(data)
		{
		}

		public UpnpRevokeMessage(Guid deviceId, NotificationType nt, string entity, int entityVersion)
			: base(deviceId, nt, entity, entityVersion)
		{
			base["NTS"] = "ssdp:byebye";
		}
	}
}
