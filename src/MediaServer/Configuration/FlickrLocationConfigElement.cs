namespace MediaServer.Configuration
{
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
}