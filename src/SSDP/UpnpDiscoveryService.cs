using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SSDP.Messages;

namespace SSDP
{
	public sealed class UpnpDiscoveryService
	{
		#region Singleton

		static UpnpDiscoveryService()
		{
		}

		private static readonly UpnpDiscoveryService SingletonInstance = new UpnpDiscoveryService();
		
		public static UpnpDiscoveryService Instance { get { return SingletonInstance; } }

		#endregion

		public event EventHandler<UpnpDiscoveryMessageReceivedEventArgs> MessageReceived;

		private const string UpnpMulticastGroup = "239.255.255.250";
		private const int UpnpPort = 1900;
		
		private readonly UdpClient _multicastGroupListenerClient;
		private readonly IPAddress _multicastGroupAddress;
		private readonly IPEndPoint _multicastGrounEndPoint;

		private UpnpDiscoveryService()
		{
			_multicastGroupAddress = IPAddress.Parse(UpnpMulticastGroup);
			if (_multicastGroupAddress == null) throw new InvalidOperationException("Invalid group address");

			_multicastGrounEndPoint = new IPEndPoint(_multicastGroupAddress, UpnpPort);
			_multicastGroupListenerClient = new UdpClient { MulticastLoopback = false, Ttl = 4 };
			_multicastGroupListenerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_multicastGroupListenerClient.Client.Bind(new IPEndPoint(IPAddress.Any, UpnpPort));
			_multicastGroupListenerClient.JoinMulticastGroup(_multicastGroupAddress);

			_multicastGroupListenerClient.BeginReceive(ReceiveHandler, null);
		}

		~UpnpDiscoveryService()
		{
			try
			{
				_multicastGroupListenerClient.DropMulticastGroup(_multicastGroupAddress);
				_multicastGroupListenerClient.Close();
			}
			catch (Exception)
			{
			}
		}

		private void ReceiveHandler(IAsyncResult res)
		{
			lock (this)
			{
				var remoteEndpoint = new IPEndPoint(0, 0);
				byte[] data;

				try
				{
					data = _multicastGroupListenerClient.EndReceive(res, ref remoteEndpoint);
				}
				catch (Exception)
				{
					return;
				}
				finally
				{
					_multicastGroupListenerClient.BeginReceive(ReceiveHandler, null);
				}

				var messageText = Encoding.ASCII.GetString(data);
				var rawMsg = UpnpMessage.Create(messageText);

				OnMessageReceived(rawMsg, remoteEndpoint);
			}
		}

		private void OnMessageReceived(UpnpMessage message, IPEndPoint remoteEndPoint)
		{
			var temp = MessageReceived;
			if (temp != null)
			{
				temp(this, new UpnpDiscoveryMessageReceivedEventArgs(message, remoteEndPoint));
			}
		}

		public void SendMessage(UpnpMessage msg)
		{
			lock (this)
			{
				using (var client = new UdpClient())
				{
					var data = Encoding.ASCII.GetBytes(msg.ToString());
					client.Send(data, data.Length, _multicastGrounEndPoint);
				}
			}
		}

		public void SendMessages(IEnumerable<UpnpMessage> msgs)
		{
			lock (this)
			{
				using (var client = new UdpClient())
				{
					foreach (var data in msgs.Select(msg => Encoding.ASCII.GetBytes(msg.ToString())))
					{
					    client.Send(data, data.Length, _multicastGrounEndPoint);
					}
				}
			}
		}


		public void SendMessage(UpnpMessage msg, IPEndPoint endpoint)
		{
			lock(this)
			{
				using (var client = new UdpClient())
				{
					var data = Encoding.ASCII.GetBytes(msg.ToString());
					client.Send(data, data.Length, endpoint);
				}
			}
		}

		public void SendMessages(IEnumerable<UpnpMessage> msgs, IPEndPoint endpoint)
		{
			lock (this)
			{
				using (var client = new UdpClient())
				{
					foreach (var data in msgs.Select(msg => Encoding.ASCII.GetBytes(msg.ToString())))
					{
					    client.Send(data, data.Length, endpoint);
					}
				}
			}
		}


	}
}
