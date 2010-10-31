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

	class FlickrUserConfigElement : FlickrConfigElement
	{
		public FlickrUserConfigElement(string label, string user)
			: base(label)
		{
			User = user;
		}
		public string User { get; private set; }
	}

	class FlickrInterestingConfigElement : FlickrConfigElement
	{
		public FlickrInterestingConfigElement(string label)
			: base(label)
		{
		}
	}

	class FlickrLocationConfigElement : FlickrConfigElement
	{
		public FlickrLocationConfigElement(string label, float latitude, float longitude, float radius)
			: base(label)
		{
			Latitude = latitude;
			Longitude = longitude;
			Radius = radius;
		}
		public float Longitude { get; private set; }
		public float Latitude { get; private set; }
		public float Radius { get; private set; }
	}

	class FlickrTextConfigElement : FlickrConfigElement
	{
		public FlickrTextConfigElement(string label, string text)
			: base(label)
		{
			Text = text;
		}
		public string Text { get; private set; }
	}

	class FlickrTagsConfigElement : FlickrConfigElement
	{
		public FlickrTagsConfigElement(string label, string tags)
			: base(label)
		{
			Tags = tags;
		}
		public string Tags { get; private set; }
	}
}