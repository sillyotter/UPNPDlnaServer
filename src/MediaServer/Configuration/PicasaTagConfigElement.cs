namespace MediaServer.Configuration
{
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