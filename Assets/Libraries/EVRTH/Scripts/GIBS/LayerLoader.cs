using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EVRTH.Scripts.WMS;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace EVRTH.Scripts.GIBS
{
    public static class LayerLoader
    {
        private const string gcPath = "XML/wmtsGC";
        private const string wmsGcPath = "XML/wmsGC";
        private const string matrixSetPath = "XML/TileMatrixSets";
        private const string defaultTileMatrixSet = "250m";

        private static string tileMatrixAsset;
        private static string wmsGcAsset;
        private static string gcAsset;

        public static void Init()
        {
            tileMatrixAsset = ((TextAsset) Resources.Load(matrixSetPath)).text;
            wmsGcAsset = ((TextAsset) Resources.Load(wmsGcPath)).text;
            gcAsset = ((TextAsset) Resources.Load(gcPath)).text;
        }

        public static Dictionary<string, Layer> GetLayers(string gcUrl = null, bool useOnline = false)
        {
            // Process TileMatrixSets
            //TextAsset 
            Debug.Assert(tileMatrixAsset != null, "tileMatrixAsset != null");
            Dictionary<string, List<TileMatrix>> tileMatrixSets = CreateTileMatrixSets(new XmlTextReader(new StringReader(tileMatrixAsset)));
            Dictionary<string, Layer> layers = new Dictionary<string, Layer>();
            // WMTS layer loading
            XmlTextReader gcReader;

            if (useOnline)
            {
                if (string.IsNullOrEmpty(gcUrl))
                {
                    gcUrl = "http://gibs.earthdata.nasa.gov/wmts/epsg4326/best/wmts.cgi?SERVICE=WMTS&request=GetCapabilities";
                }
                gcReader = new XmlTextReader(gcUrl);
            }
            else
            {
                //TextAsset gcAsset = Resources.Load(gcPath) as TextAsset;
                Debug.Assert(gcAsset != null, "gcAsset != null");
                gcReader = new XmlTextReader(new StringReader(gcAsset));
            }

            gcReader = FindNextLayer(gcReader);
            while (gcReader != null)
            {
                Layer newLayer = CreateLayer(gcReader, useOnline);

                //Assign tile matrix set to object based on identifier
                newLayer.tileMatrixSet = tileMatrixSets[newLayer.tileMatrixSetIdentifier];
                layers.Add(newLayer.identifier, newLayer);
                if (gcReader.EOF)
                {
                    gcReader.Close();
                    gcReader = null;
                }
                gcReader = FindNextLayer(gcReader);
            }

            // WMS layer loading
            //TextAsset 
            Debug.Assert(wmsGcAsset != null, "wmsGcAsset != null");
            XmlTextReader wmsGcReader = new XmlTextReader(new StringReader(wmsGcAsset));

            wmsGcReader = FindNextLayer(wmsGcReader, 3);
            while (wmsGcReader != null)
            {
                Layer newLayer = CreateWmsLayer(wmsGcReader);
                //Assign tile matrix set to object based on identifier
                newLayer.tileMatrixSet = tileMatrixSets[newLayer.tileMatrixSetIdentifier];

                if (!layers.ContainsKey(newLayer.identifier))
                {
                    layers.Add(newLayer.identifier, newLayer);
                }
                if (wmsGcReader.EOF)
                {
                    wmsGcReader.Close();
                    wmsGcReader = null;
                }
                wmsGcReader = FindNextLayer(wmsGcReader, 3);
            }

            return layers;
        }

        private static Dictionary<string, List<TileMatrix>> CreateTileMatrixSets(XmlTextReader matrixReader)
        {
            //Debug.Log ("parsing tilematrix");
            Dictionary<string, List<TileMatrix>> tileMatrixSets = new Dictionary<string, List<TileMatrix>>();
            while (matrixReader.Read())
            {
                if (matrixReader.Name.Equals("TileMatrixSet"))
                {
                    List<TileMatrix> newSet = new List<TileMatrix>();
                    string setIdentifier = null;
                    while (matrixReader.Read() && !(matrixReader.NodeType == XmlNodeType.EndElement && matrixReader.Name.Equals("TileMatrixSet")))
                    {
                        if (matrixReader.NodeType == XmlNodeType.Element && matrixReader.Name.Equals("ows:Identifier") && matrixReader.Depth == 2)
                        {
                            setIdentifier = matrixReader.ReadElementContentAsString();
                        }
                        else if (matrixReader.NodeType == XmlNodeType.Element && matrixReader.Name.Equals("TileMatrix"))
                        {

                            string identifier = null;
                            decimal scaleDenominator = 0;
                            LatLon topLeftCorner = new LatLon(0, 0);
                            int tileWidth = 0, tileHeight = 0, matrixWidth = 0, matrixHeight = 0;

                            while (matrixReader.Read() && !(matrixReader.NodeType == XmlNodeType.EndElement && matrixReader.Name.Equals("TileMatrix")))
                            {
                                if (matrixReader.NodeType == XmlNodeType.Element)
                                {
                                    if (matrixReader.Name.Equals("ows:Identifier"))
                                    {
                                        identifier = matrixReader.ReadElementContentAsString();
                                    }
                                    else if (matrixReader.Name.Equals("ScaleDenominator"))
                                    {
                                        scaleDenominator = matrixReader.ReadElementContentAsDecimal();
                                    }
                                    else if (matrixReader.Name.Equals("TopLeftCorner"))
                                    {
                                        string topLeftString = matrixReader.ReadElementContentAsString();
                                        string[] latlonArray = topLeftString.Split(' ');
                                        topLeftCorner = new LatLon(Int32.Parse(latlonArray[0]), Int32.Parse(latlonArray[1]));
                                    }
                                    else if (matrixReader.Name.Equals("TileWidth"))
                                    {
                                        tileWidth = matrixReader.ReadElementContentAsInt();
                                    }
                                    else if (matrixReader.Name.Equals("TileHeight"))
                                    {
                                        tileHeight = matrixReader.ReadElementContentAsInt();
                                    }
                                    else if (matrixReader.Name.Equals("MatrixWidth"))
                                    {
                                        matrixWidth = matrixReader.ReadElementContentAsInt();
                                    }
                                    else if (matrixReader.Name.Equals("MatrixHeight"))
                                    {
                                        matrixHeight = matrixReader.ReadElementContentAsInt();
                                    }
                                }
                            }

                            TileMatrix newMatrix = new TileMatrix { identifier = identifier, scaleDenominator = scaleDenominator, topLeftCorner = topLeftCorner, tileWidth = tileWidth, tileHeight = tileHeight, matrixWidth = matrixWidth, matrixHeight = matrixHeight };
                            int matrixKey;
                            if (int.TryParse(identifier, out matrixKey))
                            {
                                newSet.Insert(matrixKey, newMatrix);
                            }
                            else
                            {
                                UnityEngine.Debug.LogError("Invalid key found for tile matrix");
                            }

                        }
                    }
                    if (setIdentifier != null) tileMatrixSets.Add(setIdentifier, newSet);
                }
            }
            return tileMatrixSets;
        }

        private static Layer CreateLayer(XmlTextReader layerNode, bool useOnline)
        {
            string identifier = null;
            string title = null;
            string format = null;
            string tileMatrixSetIdentifier = null;
            float minLat = 0;
            float maxLat = 0;
            float minLon = 0;
            float maxLon = 0;
            LatLonBoundingBox bbox = null;
            string colormapUrl = null;

            while (layerNode.Read() && !layerNode.Name.Equals("Layer"))
            {
                switch (layerNode.NodeType)
                {
                    case XmlNodeType.Element:
                        if (layerNode.Name.Equals("ows:Title") && layerNode.Depth == 3)
                        {
                            title = layerNode.ReadElementContentAsString();
                        }
                        else if (layerNode.Name.Equals("ows:Identifier") && layerNode.Depth == 3)
                        {
                            identifier = layerNode.ReadElementContentAsString();
                        }
                        else if (layerNode.Name.Equals("Format"))
                        {
                            format = layerNode.ReadElementContentAsString();
                        }
                        else if (layerNode.Name.Equals("ows:LowerCorner"))
                        {
                            string lowerCorner = layerNode.ReadElementContentAsString();
                            string[] coords = lowerCorner.Split(' ');
                            minLat = Convert.ToSingle(coords[0]);
                            minLon = Convert.ToSingle(coords[1]);
                        }
                        else if (layerNode.Name.Equals("ows:UpperCorner"))
                        {
                            string upperCorner = layerNode.ReadElementContentAsString();
                            string[] coords = upperCorner.Split(' ');
                            maxLat = Convert.ToSingle(coords[0]);
                            maxLon = Convert.ToSingle(coords[1]);
                        }
                        else if (layerNode.Name.Equals("ows:Metadata"))
                        {
                            if (colormapUrl == null)
                            {
                                colormapUrl = layerNode.GetAttribute("xlink:href");
                            }
                        }
                        else if (layerNode.Name.Equals("Dimension"))
                        {
                            //TODO Add start/end time parsing here
                        }
                        else if (layerNode.Name.Equals("TileMatrixSet"))
                        {
                            tileMatrixSetIdentifier = layerNode.ReadElementContentAsString();
                        }
                        break;
                }
            }
            if (minLat != 0 || maxLat != 0 || minLon != 0 || maxLon != 0)
            {
                bbox = new LatLonBoundingBox(minLat, maxLat, minLon, maxLon);
            }

            Layer layer = new Layer { identifier = identifier, title = title, format = format, colormapUrl = colormapUrl, bbox = bbox, useOnline = useOnline, tileMatrixSetIdentifier = tileMatrixSetIdentifier };
            return layer;
        }

        private static Layer CreateWmsLayer(XmlTextReader layerNode)
        {
            Layer layer = null;

            string identifier = null;
            string title = null;
            //string format = null;
            //float minLat = 0;
            //float maxLat = 0;
            //float minLon = 0;
            //float maxLon = 0;
            //LatLonBoundingBox bbox = null;
            //string colormapUrl = null;

            while (layerNode.Read() && !layerNode.Name.Equals("Layer"))
            {
                switch (layerNode.NodeType)
                {
                    case XmlNodeType.Element:
                        if (layerNode.Name.Equals("Title") && layerNode.Depth == 4)
                        {
                            title = layerNode.ReadElementContentAsString();
                        }
                        else if (layerNode.Name.Equals("Name") && layerNode.Depth == 4)
                        {
                            identifier = layerNode.ReadElementContentAsString();
                        }
                        break;
                }
                layer = new Layer { identifier = identifier, title = title, tileMatrixSetIdentifier = defaultTileMatrixSet, isWms = true };

            }
            return layer;
        }

        private static XmlTextReader FindNextLayer(XmlTextReader reader, int depth = 2)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Depth == depth && reader.Name.Equals("Layer"))
                        {
                            return reader;
                        }
                        break;
                }
            }
            return null;
        }
    }
}
