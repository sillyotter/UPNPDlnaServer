using Google.GData.Photos;

namespace MediaServer.Media.Nodes.Picassa
{
	abstract class PicasaFolderBase : WebResourceFolderBase
	{
		protected readonly PicasaService Service = new PicasaService("oliversoft-MediaServer-1.0");

		protected PicasaFolderBase(FolderNode parentNode, string title) 
			: base(parentNode, title)
		{
			NumberToRetrieve = 100;
		}

		public int NumberToRetrieve { get; set; }

	}
}