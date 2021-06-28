using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



namespace EVRTH.Scripts.GIBS
{
    public class LayersDropdown : MonoBehaviour
    {
        //For getting values of WMS Layer
        public WMSLayers wmslayer_script;
        public GameObject WMSObjects;

        public GameObject WMSGlobe, WMSPlane;

        public Dropdown layerDropdown;              // Dropdown GameObject
        private string gcUrl_v_1_3_0 = "https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetCapabilities&VERSION=1.3.0";
        public List<string> fullLayerList;          // List has all the layer names before user searches for specific ones
        public string layerName = "";               // The specific layer selected from Dropdown menu
     
        public int layerDropdownValue;              // Index value of the dropdown menu

        
        bool defaultOptions = true;

        public List<string> demoList;

        public Material materialGlobe, materialPlane;

        double[] bBox = { -180, -90, 180, 90 };




        // Start is called before the first frame update
        void Start()
        {

            layerDropdown = transform.GetComponent<Dropdown>();

            // Get the WMSLayer Script
            //wmslayer_script = WMSObjects.GetComponent<WMSLayers>();

            layerDropdown.options.Clear();

            fullLayerList = WMSLayers.LayerList(gcUrl_v_1_3_0);

            //Debug.Log(allLayerInfo);


            //fullLayerList = wmslayer_script.getLayerIdentifiers(allLayerInfo);

            foreach (string option in fullLayerList)
            {
                //Debug.Log(option);
                // Take layer string from layerList and put into dropdown menu
                layerDropdown.options.Add(new Dropdown.OptionData() { text = option });
            }

            layerDropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(layerDropdown); });

        }


        private void DropdownItemSelected(Dropdown layerDropdown)
        {
            layerDropdownValue = layerDropdown.value;

            Debug.Log(layerDropdownValue);

            layerName = fullLayerList[layerDropdownValue];

            Debug.Log(layerName);

            StartCoroutine(getRequest(layerName));


        }


        IEnumerator getRequest(string layerName)
        {

            materialGlobe = WMSGlobe.GetComponent<Renderer>().material;
            materialPlane = WMSPlane.GetComponent<Renderer>().material;

            WMSRequest request = new WMSRequest(layerName, 2048, 2048, bBox, "default", "4326", "image/png", "https://gibs.earthdata.nasa.gov", "");

            Texture2D texture;
            Debug.Log(request.Url);
            texture = new Texture2D(2048, 2048);

            using (WWW wmsWeb = new WWW(request.Url))
            {
                yield return wmsWeb;
                wmsWeb.LoadImageIntoTexture(texture);
                materialGlobe.SetTexture("_MainTex", texture);
                materialPlane.SetTexture("_MainTex", texture);
            }
        }



        // Update is called once per frame
        void Update()
        {
  
        }

    }

}


