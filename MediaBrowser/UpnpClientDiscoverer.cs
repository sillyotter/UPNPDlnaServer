using System;
using System.Collections.Generic;
using System.Net;
using SSDP;
using SSDP.Messages;

namespace MediaBrowser
{
	public class UpnpClientDiscoverer
	{
		public event EventHandler<EventArgs> UpnpServerDiscovered;

		private void InvokeUpnpServerDiscovered(EventArgs e)
		{
			var discovered = UpnpServerDiscovered;
			if (discovered != null) discovered(this, e);
		}

		private readonly IList<IPEndPoint> _foundEndpoints = new List<IPEndPoint>();

		public IEnumerable<IPEndPoint> Servers { get { return _foundEndpoints; } }

		public UpnpClientDiscoverer()
		{
			UpnpDiscoveryService.Instance.MessageReceived += MessageReceived;
		}

		public void SearchForAllEntites()
		{
			UpnpDiscoveryService.Instance.SendMessage(new UpnpSearchMessage(SearchType.All, TimeSpan.FromSeconds(1), String.Empty, 0, Guid.Empty));
		}

		public void Clear()
		{
			_foundEndpoints.Clear();
		}

		void MessageReceived(object sender, UpnpDiscoveryMessageReceivedEventArgs e)
		{
			var msg = e.Message as UpnpSearchResponse;
			if (msg != null)
			{
				if (!_foundEndpoints.Contains(e.Source))
				{
					_foundEndpoints.Add(e.Source);
					InvokeUpnpServerDiscovered(EventArgs.Empty);
				}
			}
		}



	}
}
