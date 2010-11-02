namespace MediaServer.Configuration
{
    class PicasaUserConfigElement : PicasaConfigElement
    {
        public PicasaUserConfigElement(string label, string userid)
            : base(label)
        {
            UserId = userid;
        }

        public string UserId { get; private set; }
    }
}