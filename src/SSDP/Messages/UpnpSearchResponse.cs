using System;

namespace SSDP.Messages
{
	public class UpnpSearchResponse : UpnpMessage
	{
		public UpnpSearchResponse(string data)
			: base(data)
		{
		}

		public UpnpSearchResponse(TimeSpan expiration, Uri infoLink, 
		                          Guid deviceId, NotificationType nt, string entity, int entityVersion, string serverName)
		{
			FirstLine = "HTTP/1.1 200 OK";
			base["CACHE-CONTROL"] = String.Format("max-age = {0}", (int)expiration.TotalSeconds);
			base["DATE"] = DateTime.Now.ToUniversalTime().ToString("R");
			base["EXT"] = String.Empty;
			base["LOCATION"] = infoLink.ToString();
			base["SERVER"] = serverName;
			
			var notificationType = "";
			switch (nt)
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

			base["ST"] = notificationType;

			if (notificationType.StartsWith("uuid"))
			{
				base["USN"] = "uuid:" + deviceId;
			}
			else
			{
				base["USN"] = "uuid:" + deviceId + "::" + notificationType;
			}
		}

		public string SearchTarget
		{
			get
			{
				return base["ST"];
			}
		}

		public TimeSpan Expiration
		{
			get
			{
				var cc = base["CACHE-CONTROL"];
				var res = cc.Split(' ');
				return res.Length == 3 ? TimeSpan.FromSeconds(double.Parse(res[2])) : TimeSpan.Zero;
			}
		}

		public Uri InfoLink
		{
			get
			{
				return new Uri(base["LOCATION"]);
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