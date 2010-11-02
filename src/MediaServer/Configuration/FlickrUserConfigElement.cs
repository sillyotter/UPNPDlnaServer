namespace MediaServer.Configuration
{
    class FlickrUserConfigElement : FlickrConfigElement
    {
        public FlickrUserConfigElement(string label, string user)
            : base(label)
        {
            User = user;
        }

        public string User { get; private set; }
    }
}