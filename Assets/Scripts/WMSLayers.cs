using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EVRTH.Scripts.WMS;
using UnityEngine;
//using Debug = System.Diagnostics.Debug;
using Debug = UnityEngine.Debug;


// Some of this is adapted from "LayerLoader.cs" script
namespace EVRTH.Scripts.GIBS
{

    public class WMSLayers : MonoBehaviour
    {
        private const string wmsGcPath = "XML/wmsGC";       // To access WMS GetCapabilities file saved on local machine (inside Unity project)
        private static string wmsGcAsset;
        private const string defaultTileMatrixSet = "250m";

        // URLs to GetCapabilities files
        private string gcUrl_v_1_1_1 = "https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetCapabilities&VERSION=1.1.1";
        private string gcUrl_v_1_3_0 = "https://gibs.earthdata.nasa.gov/wms/epsg4326/best/wms.cgi?SERVICE=WMS&REQUEST=GetCapabilities&VERSION=1.3.0";

 

        public static List<string> LayerList(string gcUrl)
        {
            Debug.Log(gcUrl);       // Ensures reading in URL correctly

            // List of layers
            List<string> layers = new List<string>();

            // Create instance of XMLTextReader and plug in URL
            XmlTextReader wmsGcReader = new XmlTextReader(/*new StringReader*/(gcUrl));

            int depth = 3;

            // Check if under a <Layer> node
            wmsGcReader = FindNextLayer(wmsGcReader);

            while (wmsGcReader != null)
            {

                string newLayer = CreateWmsLayer(wmsGcReader, depth);

                // Show content of newLayer
                //Debug.Log(newLayer);

                if (!layers.Contains(newLayer))
                {
                    layers.Add(newLayer);
                }

                // If there are no more nodes to read
                if (wmsGcReader.EOF)
                {
                    wmsGcReader.Close();
                    wmsGcReader = null;
                }

                // Check to see if this node has a child node of <Layer> too
                wmsGcReader = FindNextLayer(wmsGcReader);
            }
            return layers;
        }




        // Ensures only the Layer trees are parsed for layer info
        public static XmlTextReader FindNextLayer(XmlTextReader reader/*, int depth = 2*/)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Depth > 2 && reader.Name.Equals("Layer"))
                        {
                            return reader;
                        }
                        break;
                }
            }
            return null;
        }




        public static string CreateWmsLayer(XmlTextReader layerNode, int depth)
        {
            Layer layer = null;

            string identifier = null;
            string title = null;

            //Debug.Log(layerNode.Read());

            while (layerNode.Read() && !layerNode.Name.Equals("Layer"))
            {
                switch (layerNode.NodeType)
                {
                    case XmlNodeType.Element:

                        if (layerNode.Name.Equals("Title") && layerNode.Depth > depth)
                        {
                            title = layerNode.ReadElementContentAsString();
                        }

                        else if (layerNode.Name.Equals("Name") && layerNode.Depth > depth)
                        {
                            identifier = layerNode.ReadElementContentAsString();
                        }
                        break;
                }
                //layer = new Layer { identifier = identifier, title = title, tileMatrixSetIdentifier = defaultTileMatrixSet, isWms = true };

            }
            return identifier;
        }




        public List<string> getLayerIdentifiers(Dictionary<string, Layer> allLayerInfo)
        {
            List<string> layerIdentifiers = new List<string>();

            // Populate an ArrayList with layer names (identifiers)
            foreach (string s in allLayerInfo.Keys)
            {
                layerIdentifiers.Add(s);
            }

            return layerIdentifiers;

        }


    }

}
