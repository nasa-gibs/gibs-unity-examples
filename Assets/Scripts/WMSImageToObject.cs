//Creates a WMSRequest and translates it into a texture, which is then applied to a material assigned to the script by the Unity Inspector
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;



namespace EVRTH.Scripts.GIBS
{
    public class WMSImageToObject : MonoBehaviour
    {
        double[] bBox = { -180, -90, 180, 90 };
        public Material material;

        public string layerName;

        public GameObject DropdownMenu;

        public LayersDropdown script_layersdropdown;


        IEnumerator Start()
        {
            //wmslayers_script = GetComponent<WMSLayers>();

            // Place to choose
            layerName = "VIIRS_SNPP_CorrectedReflectance_TrueColor";

            /*script_layersdropdown = DropdownMenu.GetComponent<LayersDropdown>();

            if (script_layersdropdown.layerName == "")
            {
                layerName = "VIIRS_SNPP_CorrectedReflectance_TrueColor";
            }
            
            else
            {
                layerName = script_layersdropdown.layerName;
            }
            */

            Debug.Log(layerName);


            //Makes a WMSRequest Object with the parameters needed to display the Daily Surface Temp Layer)
            //Uses components from the following URL 
            //https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetMap&VERSION=1.3.0&LAYERS=MODIS_Terra_SurfaceReflectance_Bands721&STYLES=&FORMAT=image%2Fpng&TRANSPARENT=true&HEIGHT=256&WIDTH=256&TIME=2018-10-01&CRS=EPSG:4326&BBOX=-22.5,0,0,22.5
            WMSRequest request = new WMSRequest(layerName, 2048, 2048, bBox, "2018-10-01", "4326", "image/png", "https://gibs.earthdata.nasa.gov", "");

            Texture2D texture;
            Debug.Log(request.Url);
            texture = new Texture2D(2048, 2048);

            using (WWW wmsWeb = new WWW(request.Url))
            {
                yield return wmsWeb;
                wmsWeb.LoadImageIntoTexture(texture);
                material.SetTexture("_MainTex", texture);
            }
        }


    }

}
