using System;

namespace SSDP.Messages
{
	public class UpnpAnnounceMessage : UpnpAdvertiseMessage
	{
		public UpnpAnnounceMessage(string data)
			: base(data)
		{
		}

		public UpnpAnnounceMessage(Guid deviceId, NotificationType nt, string entity, int entityVersion,
		                           TimeSpan expiration, Uri infoLink, string serverName)
			: base(deviceId, nt, entity, entityVersion)
		{
			base["NTS"] = "ssdp:alive";
			base["SERVER"] = serverName;
			base["LOCATION"] = infoLink.ToString();
			base["CACHE-CONTROL"] = String.Format("max-age = {0}", (int)expiration.TotalSeconds);
		}

		public Uri InfoLink
		{
			get
			{
				return new Uri(base["LOCATION"]);
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
	}
}