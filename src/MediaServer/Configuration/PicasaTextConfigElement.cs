namespace MediaServer.Configuration
{
    class PicasaTextConfigElement : PicasaConfigElement
    {
        public PicasaTextConfigElement(string label, string query)
            : base(label)
        {
            Query = query;
        }

        public string Query { get; private set; }
    }
}