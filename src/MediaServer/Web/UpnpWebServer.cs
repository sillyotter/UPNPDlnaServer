using System.Collections.Generic;

namespace MediaServer.Web
{
	class UpnpWebServer : WebServer
	{
		private readonly ConnectionManager _connectionManager = new ConnectionManager();
		private readonly ContentDirectory _contentDirectory = new ContentDirectory();
		private readonly StaticFileHandler _staticFileHandler = new StaticFileHandler();
		private readonly MusicImageHandler _musicImageHandler = new MusicImageHandler();
		//private readonly ImageThumbnailHandler _imageThumbnailHandler = new ImageThumbnailHandler();
		//private readonly MediaHandler _mediaHandler = new MediaHandler();
		//private readonly ApiHandler _apiHandler = new ApiHandler();

		public UpnpWebServer()
		{
			RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/Connection/Control", _connectionManager));
			RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/Content/Control", _contentDirectory));
			RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/GetMusicImage.*", _musicImageHandler));
			//RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/GetImageThumbnail.*", _imageThumbnailHandler));
			//RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/GetMedia.*", _mediaHandler));
			//RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/MediaServer/api/.*", _apiHandler));
			RequestHandlers.Add(new KeyValuePair<string, BaseRequestHandler>("/.*", _staticFileHandler));
		}
	}
}
