using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MediaServer.Configuration;
using SSDP;
using SSDP.Messages;

namespace MediaServer.SSDP
{
	public class UpnpMediaServerMessageHandler
	{
		private const int ExpirationSeconds = 1800;

		private readonly Timer _aliveTimer;
		private readonly IList<UpnpMessage> _announcementMessages;
		private readonly IList<UpnpMessage> _revokeMessages;
		private readonly Guid _deviceId;

		private static IEnumerable<Uri> LocalWebAddresses
		{
			get
			{
				return from address in Dns.GetHostEntry(Dns.GetHostName()).AddressList
					   where address.AddressFamily == AddressFamily.InterNetwork
					   select new Uri("http://" + address + ":" + Settings.Instance.QueryPort + "/MediaServer/");
			}
		}

		public UpnpMediaServerMessageHandler()
		{
			_aliveTimer = new Timer(OnTimerCallback);
			_deviceId = Settings.Instance.DeviceId;

			var query = from address in LocalWebAddresses
			            let descriptionLink = new Uri(address, "DeviceDescription.xml")
			            select
			            	new List<UpnpMessage>
			            		{
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.RootDevice, String.Empty, 0,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), address,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.DeviceId, String.Empty, 0,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), address,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.DeviceType, "MediaServer", 1,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), descriptionLink,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.DeviceId, String.Empty, 0,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), address,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.DeviceType, "MediaServer", 1,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), descriptionLink,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.ServiceType, "ContentDirectory", 1,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), descriptionLink,
			            			                        Settings.Instance.ServerName),
			            			new UpnpAnnounceMessage(_deviceId, NotificationType.ServiceType, "ConnectionManager", 1,
			            			                        TimeSpan.FromSeconds(ExpirationSeconds), descriptionLink,
			            			                        Settings.Instance.ServerName),

			            		};

			_announcementMessages = query.SelectMany(item => item).ToList();

			_revokeMessages =
				new List<UpnpMessage>
					{
						new UpnpRevokeMessage(_deviceId, NotificationType.RootDevice, String.Empty, 0),
						new UpnpRevokeMessage(_deviceId, NotificationType.DeviceId, String.Empty, 0),
						new UpnpRevokeMessage(_deviceId, NotificationType.DeviceType, "MediaServer", 1),
						new UpnpRevokeMessage(_deviceId, NotificationType.DeviceId, String.Empty, 0),
						new UpnpRevokeMessage(_deviceId, NotificationType.DeviceType, "MediaServer", 1),
						new UpnpRevokeMessage(_deviceId, NotificationType.ServiceType, "ContentDirectory", 1),
						new UpnpRevokeMessage(_deviceId, NotificationType.ServiceType, "ConnectionManager", 1)
					};

			UpnpDiscoveryService.Instance.MessageReceived += MessageReceived;
		}

		~UpnpMediaServerMessageHandler()
		{
			UpnpDiscoveryService.Instance.MessageReceived -= MessageReceived;
		}

		void MessageReceived(object sender, UpnpDiscoveryMessageReceivedEventArgs e)
		{
			var msg = e.Message as UpnpSearchMessage;

			if (msg == null) return;

			if (msg.DeviceId == _deviceId || msg.DeviceId == Guid.Empty)
			{
				var responses = new List<UpnpSearchResponse>(10);
			
				if (msg.SearchType == SearchType.All)
				{
					var query = from infoLink in LocalWebAddresses
								let deviceLink = new Uri(infoLink, "DeviceDescription.xml")
								select new[] {
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), infoLink, _deviceId,
														NotificationType.RootDevice, String.Empty, 0, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), infoLink, _deviceId,
														NotificationType.DeviceId, String.Empty, 0, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), deviceLink, _deviceId,
														NotificationType.DeviceType, "MediaServer", 1, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), infoLink, _deviceId,
														NotificationType.DeviceId, String.Empty, 0, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), deviceLink, _deviceId,
														NotificationType.DeviceType, "MediaServer", 1, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), deviceLink, _deviceId,
														NotificationType.ServiceType, "ContentDirectory", 1, Settings.Instance.ServerName),
									new UpnpSearchResponse(TimeSpan.FromSeconds(ExpirationSeconds), deviceLink, _deviceId,
														NotificationType.ServiceType, "ConnectionManager", 1, Settings.Instance.ServerName)
								};
					responses.AddRange(query.SelectMany(item=>item).ToList());
				}
				else
				{
					if (msg.SearchType == SearchType.ServiceType && msg.Entity != "ContentDirectory") return;
					if (msg.SearchType == SearchType.DeviceType && msg.Entity != "MediaServer") return;

					var query = from infoLink in LocalWebAddresses
								let deviceLink = new Uri(infoLink, "DeviceDescription.xml")
								select new UpnpSearchResponse(
										TimeSpan.FromSeconds(ExpirationSeconds), 
										deviceLink, 
										_deviceId,
										(NotificationType)msg.SearchType, 
										msg.Entity, 
										msg.EntityVersion, 
										Settings.Instance.ServerName);

					responses.AddRange(query.ToList());
				}

				var rand = new Random();

				var delay = (int)msg.Delay.TotalMilliseconds;
				foreach (var item in responses)
				{
					var ld = rand.Next(delay);
					delay -= ld;
					Thread.Sleep(ld);
					UpnpDiscoveryService.Instance.SendMessage(item, e.Source);
				}
			}
		}

		private void OnTimerCallback(object state)
		{
			var rand = new Random();
			foreach (var message in _announcementMessages)
			{
				Thread.Sleep(rand.Next(100));
				UpnpDiscoveryService.Instance.SendMessage(message);
			}

			_aliveTimer.Change(TimeSpan.FromSeconds(rand.Next(ExpirationSeconds / 2)), TimeSpan.FromMilliseconds(-1));
		}

		public void Start()
		{
			_aliveTimer.Change(TimeSpan.FromSeconds(0), TimeSpan.FromMilliseconds(-1));
		}

		public void Stop()
		{
			var rand = new Random();
			foreach (var message in _revokeMessages)
			{
				Thread.Sleep(rand.Next(100));
				UpnpDiscoveryService.Instance.SendMessage(message);
			}
			_aliveTimer.Change(-1, -1);
		}

	}
}
