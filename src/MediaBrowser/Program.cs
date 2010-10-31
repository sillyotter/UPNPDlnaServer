using System;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Threading;

namespace MediaBrowser
{
	class Program
	{
		static void Main(string[] args)
		{
			Thread.Sleep(2000);
			var req = WebRequest.Create("http://localhost:12345/MediaServer/GetMedia?id=00000000-0000-0000-0000-000000000002");
			req.Method = "HEAD";
			var resp = req.GetResponse();
			Console.WriteLine(resp.ContentLength);

			for (var i = 0; i < 10; ++i)
			{
				Console.WriteLine("Getting chunk");
				var req2 = WebRequest.Create("http://localhost:12345/MediaServer/GetMedia?id=00000000-0000-0000-0000-000000000002") as HttpWebRequest;
				if (req2 != null)
				{
					req2.Method = "GET";
					var len = (int) Math.Min(resp.ContentLength, Int32.MaxValue);
					req2.AddRange(0 + i*1000, len);
					var resp2 = req2.GetResponse();
					using (var stream = resp2.GetResponseStream())
					{
						var buf = new byte[16*1024];
						var c = 1;
						int count = 0;
						while (c > 0)
						{
							c = stream.Read(buf, 0, 16*1024);
							if (count++ > 10000)
							{
								req2.Abort();
								break;
							}
						}
					}
				}
				Thread.Sleep(1000);
			}
		}
	}
}
