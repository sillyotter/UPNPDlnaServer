
namespace MediaServer.Web
{
	internal class Lighttpd
	{
		public Lighttpd()
		{
			UrlMapping = new Dictionary<string,string>();
		}

		public int Port { get; set; }
		public IDictionary UrlMapping { get; private set;}
	}
}
