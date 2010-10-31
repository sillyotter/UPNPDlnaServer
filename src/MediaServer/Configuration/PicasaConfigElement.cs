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

	class PicasaFeaturedConfigElement : PicasaConfigElement
	{
		public PicasaFeaturedConfigElement(string label)
			: base(label)
		{
		}
	}

	class PicasaUserConfigElement : PicasaConfigElement
	{
		public PicasaUserConfigElement(string label, string userid)
			: base(label)
		{
			UserId = userid;
		}

		public string UserId { get; private set; }
	}

	class PicasaTextConfigElement : PicasaConfigElement
	{
		public PicasaTextConfigElement(string label, string query)
			: base(label)
		{
			Query = query;
		}

		public string Query { get; private set; }
	}

	class PicasaTagConfigElement : PicasaConfigElement
	{
		public PicasaTagConfigElement(string label, string query)
			: base(label)
		{
			Query = query;
		}

		public string Query { get; private set; }
	}
}