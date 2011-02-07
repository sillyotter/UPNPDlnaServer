using System;

namespace MediaServer.SSDP.Messages
{
	public abstract class UpnpAdvertiseMessage : UpnpMessage
	{
		protected UpnpAdvertiseMessage(string data)
			: base(data)
		{
		}

		protected UpnpAdvertiseMessage(Guid deviceId, NotificationType nt, string entity, int entityVersion)
		{
			FirstLine = "NOTIFY * HTTP/1.1";
			base["HOST"] = "239.255.255.250:1900";
			
			var notificationType = "";
			switch(nt)
			{
				case NotificationType.RootDevice:
					notificationType = "upnp:rootdevice";
					break;
				case NotificationType.DeviceId:
					notificationType = "uuid:" + deviceId;
					break;
				case NotificationType.DeviceType:
					notificationType = "urn:schemas-upnp-org:device:" + entity + ":" + entityVersion;
					break;
				case NotificationType.ServiceType:
					notificationType = "urn:schemas-upnp-org:service:" + entity + ":" + entityVersion;
					break;
			}
			base["NT"] = notificationType;
			
			if (notificationType.StartsWith("uuid"))
			{
				base["USN"] = "uuid:" + deviceId;
			}
			else
			{
				base["USN"] = "uuid:" + deviceId + "::" + notificationType;
			}
		}

		public NotificationType NotificationType
		{
			get
			{
				var notificationType = base["NT"];
				if (notificationType.StartsWith("upnp")) return NotificationType.RootDevice;
				if (notificationType.StartsWith("uuid")) return NotificationType.DeviceId;
				if (notificationType.StartsWith("urn:schemas-upnp-org:device")) return NotificationType.DeviceType;
				return notificationType.StartsWith("urn:schemas-upnp-org:service") ? NotificationType.ServiceType : NotificationType.Unknown;
			}
		}

		public Guid DeviceId
		{
			get
			{
				return new Guid(base["USN"].Split(':')[1]);
			}
		}

		public string Entity
		{
			get
			{
				var notificationType = base["NT"];
				var pieces = notificationType.Split(':');
				return pieces.Length == 5 ? pieces[2] : "Unknown";
			}
		}

		public int EntityVersion
		{
			get
			{
				var notificationType = base["NT"];
				var pieces = notificationType.Split(':');
				return pieces.Length == 5 ? Int32.Parse(pieces[4]) : -1;
			}
		}
	}
}
