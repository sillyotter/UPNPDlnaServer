namespace MediaServer.Configuration
{
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