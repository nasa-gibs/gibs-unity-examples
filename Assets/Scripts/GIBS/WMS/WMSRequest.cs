using System.Reflection;

public class WMSRequest
{
    public readonly string server;
    public readonly string layer;
    public readonly string format;
    public readonly string styles;
    public readonly string time;
    public readonly int width;
    public readonly int height;
    public readonly double[] bBox;
    public readonly string projection;

    public WMSRequest(string layer, int width, int height, double[] bBox, string time, string projection = "4326", string format = "image/jpeg", string server = "https://gibs.earthdata.nasa.gov", string styles = "")
    {
        this.server = server;
        this.layer = layer;
        this.format = format;
        this.styles = styles;
        this.time = time;
        this.width = width;
        this.height = height;
        this.projection = projection;
        this.bBox = bBox;
    }

    public string Url
    {
        get
        {
            const string urlTemplate = "{0}/wms/epsg{1}/all/wms.cgi?LAYERS={2}&FORMAT={3}&TRANSPARENT=TRUE&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&STYLES={4}&SRS=EPSG%3A{5}&WIDTH={6}&HEIGHT={7}&BBOX={8},{9},{10},{11}&TIME={12}";
            return string.Format(urlTemplate, server, projection, layer, format, styles, projection, width, height, bBox[0], bBox[1], bBox[2], bBox[3], time);
            //https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0&LAYERS=MODIS_Terra_SurfaceReflectance_Bands721&STYLES=&FORMAT=image%2Fpng&TRANSPARENT=true&HEIGHT=256&WIDTH=256&TIME=2018-10-01&CRS=EPSG:4326&BBOX=-22.5,0,0,22.5
        }
    }
}