//Creates a WMSRequest and translates it into a texture, which is then applied to a material assigned to the script by the Unity Inspector
using System.Collections;
using System.IO;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

public class WMSImageToObject : MonoBehaviour
{
    public string server;
    public string layer;
    public string format;
    public string styles;
    public string time;
    public int width;
    public int height;
    double[] bBox = { -180, -90, 180, 90 };
    public string projection;
    public Material material;

    IEnumerator Start()
    {
        //Makes a WMSRequest Object with the parameters needed to display the Daily Surface Temp Layer)
        //Uses components from the following URL 
        //https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0&LAYERS=MODIS_Terra_SurfaceReflectance_Bands721&STYLES=&FORMAT=image%2Fpng&TRANSPARENT=true&HEIGHT=256&WIDTH=256&TIME=2018-10-01&CRS=EPSG:4326&BBOX=-22.5,0,0,22.5
        // Example of a pre-initialized WMSRequest:
        WMSRequest request = new WMSRequest("VIIRS_SNPP_CorrectedReflectance_TrueColor,MODIS_Terra_L3_Land_Surface_Temp_Daily_Day", 2048, 2048, bBox, "2018-10-01", "4326", "image/png", "https://gibs.earthdata.nasa.gov", "");
        //WMSRequest request = new WMSRequest(layer, width, height, bBox, time, projection = "4326", format = "image/jpeg", server = "https://gibs.earthdata.nasa.gov", styles = "");

        Texture2D texture = new Texture2D(2048, 2048); ;
        Debug.Log(request.Url);

        using (WWW wmsWeb = new WWW(request.Url))
        {
            yield return wmsWeb;
            wmsWeb.LoadImageIntoTexture(texture);
            material.SetTexture("_MainTex", texture);
        }
    }

    /*void XmlReader()
    {
        XmlDocument WMSxml = new XmlDocument();
        XmlDocument.Load(new StringReader(xmlData));

        string xmlPathPattern = "//Capability/Layer/Layer";
        XmlNodeList Layers = WMSxml.SelectNodes(xmlPathPattern);
        
        foreach(XmlNode node in Layers)
        {
            XmlNode name = node.FirstChild;
            XmlNode title = name.NextSibling;
        }


    }*/
}
