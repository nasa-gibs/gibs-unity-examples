namespace EVRTH.Scripts.WMS
{
    /// <summary>
    /// formats the web requests to the earth data servers
    /// </summary>
    public class WmsRequest
    {

        public readonly string server;
        public readonly string layer;
        public readonly string format;
        public readonly string styles;
        public readonly string time;
        public readonly int width;
        public readonly int height;
        public readonly LatLonBoundingBox bBox;

        public WmsRequest(string layer, int width, int height, LatLonBoundingBox bbox, string time,
            string server = "http://map1.vis.earthdata.nasa.gov", string format = "image/jpeg", string styles = "")
        {
            this.server = server;
            this.layer = layer;
            this.format = format;
            this.styles = styles;
            this.time = time;
            this.width = width;
            this.height = height;
            bBox = bbox;

        }

        public string Url
        {
            get
            {
                const string urlTemplate = "{0}/twms-geo/twms.cgi?request=GetMap&layers={1}&srs=EPSG:4326&format={2}&styles={3}&time={4}&width={5}&height={6}&bbox={7},{8},{9},{10}";
                return string.Format(urlTemplate, server, layer, format, styles, time, width, height, bBox.minLon,
                    bBox.minLat,
                    bBox.maxLon, bBox.maxLat);
            }
        }
    }
}
