namespace MediaServer.Configuration
{
    class FlickrTextConfigElement : FlickrConfigElement
    {
        public FlickrTextConfigElement(string label, string text)
            : base(label)
        {
            Text = text;
        }

        public string Text { get; private set; }
    }
}