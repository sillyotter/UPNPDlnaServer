namespace MediaServer.Configuration
{
	abstract class PicasaConfigElement
	{
		protected PicasaConfigElement(string label)
		{
			Label = label;
		}

		public string Label { get; private set; }
	}
}