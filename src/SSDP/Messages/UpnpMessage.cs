using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SSDP.Messages
{
	public class UpnpMessage
	{
		private readonly Dictionary<string, string> _headers = new Dictionary<string, string>(10);

		protected UpnpMessage()
		{
		}

		protected UpnpMessage(string input)
		{
			var lines = input.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			FirstLine = lines.Take(1).FirstOrDefault();
			var headers = lines.Skip(1).Select(item => item.Split(new[] { ':' }, 2))
				.Where(item => item.Length == 2);

			foreach (var item in headers)
			{
				_headers.Add(item[0], item[1]);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder(512);
			sb.Append(FirstLine + "\r\n");
			foreach (var item in _headers)
			{
				sb.AppendFormat("{0}: {1}\r\n", item.Key.ToUpperInvariant(), item.Value);
			}
			sb.Append("\r\n");

			return sb.ToString();
		}

		protected string FirstLine { get; set; }

		protected string this[string key]
		{
			get
			{
				return _headers[key].Trim();
			}
			set
			{
				_headers[key] = value;
			}
		}

		public static UpnpMessage Create(string data)
		{
			if (data.Contains("M-SEARCH")) return new UpnpSearchMessage(data);
			if (data.Contains("ssdp:alive")) return new UpnpAnnounceMessage(data);
			if (data.Contains("ssdp:byebye")) return new UpnpRevokeMessage(data);
			return data.Contains("200 OK") ? new UpnpSearchResponse(data) : new UpnpMessage(data);
		}

	}
}