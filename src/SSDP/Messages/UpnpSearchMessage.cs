using System;

namespace SSDP.Messages
{
	public class UpnpSearchMessage : UpnpMessage
	{
		public UpnpSearchMessage(string data)
			: base(data)
		{
		}

		public UpnpSearchMessage(SearchType st, TimeSpan delay, string entity, int entversion, Guid deviceId)
		{
			FirstLine = "M-SEARCH * HTTP/1.1";
			base["MAN"] = "\"ssdp:discover\"";
			base["MX"] = ((int)delay.TotalSeconds).ToString();

			var searchType = "";
			switch (st)
			{
				case SearchType.All:
					searchType = "ssdp:all";
					break;
				case SearchType.RootDevice:
					searchType = "upnp:rootdevice";
					break;
				case SearchType.DeviceId:
					searchType = "uuid:" + deviceId;
					break;
				case SearchType.DeviceType:
					searchType = "urn:schemas-upnp-org:device:" + entity + ":" + entversion;
					break;
				case SearchType.ServiceType:
					searchType = "urn:schemas-upnp-org:service:" + entity + ":" + entversion;
					break;
			}

			base["ST"] = searchType;
		}

		public TimeSpan Delay
		{
			get
			{
				return TimeSpan.FromSeconds(double.Parse(base["MX"]));
			}
		}

		public SearchType SearchType
		{
			get
			{
				var st = base["ST"];
				if (st.StartsWith("ssdp")) return SearchType.All;
				if (st.StartsWith("upnp")) return SearchType.RootDevice;
				if (st.StartsWith("uuid")) return SearchType.DeviceId;
				if (st.Contains(":device:")) return SearchType.DeviceType;
				return st.Contains(":service:") ? SearchType.ServiceType : SearchType.Unknown;
			}
		}

		public string Entity
		{
			get
			{
				var st = base["ST"];
				if (st.StartsWith("urn:schemas-upnp-org:"))
				{
					var uri = st.Split(':');
					return uri[3];
				}
				return String.Empty;
			}
		}

		public int EntityVersion
		{
			get
			{
				var st = base["ST"];
				if (st.StartsWith("urn:schemas-upnp-org:"))
				{
					var uri = st.Split(':');
					return Int32.Parse(uri[4]);
				}
				return 0;
			}
		}

		public Guid DeviceId
		{
			get
			{
				var st = base["ST"];
				if (st.StartsWith("uuid:"))
				{
					var uri = st.Split(':');
					return new Guid(uri[1]);
				}
				return Guid.Empty;
			}
		}
	}
}