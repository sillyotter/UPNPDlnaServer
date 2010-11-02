namespace MediaServer.Configuration
{
	abstract class FlickrConfigElement
	{
		protected FlickrConfigElement(string label)
		{
			Label = label;
		}

		public string Label { get; private set; }
	}
}