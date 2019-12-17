using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEngine;

namespace EVRTH.Scripts.GIBS
{
    public class ColorMap
    {
        public Dictionary<Color, DataRange> colors;
        public string url;
        public string title;
        public string units;

        private readonly bool useOnline;
        private const string colormapBasePath = "XML/Colormaps";
        public float min;
        public float max;

        // Initialize class
        public ColorMap(string url, bool useOnline = false)
        {
            this.url = url;
            this.useOnline = useOnline;
            GenerateColormap();
            List<float> minMax = GetMinMax();
            min = minMax[0];
            max = minMax[1];
//        Debug.Log("Got min of " + min + " in " + this.url);
//        Debug.Log("Got max of " + max + " in " + this.url);
        }

        // Returns the minimum and maximum values for the entire color map
        private List<float> GetMinMax()
        {
            DataRange[] dataRanges = colors.Values.ToArray();
            float tempMin = Mathf.Infinity;
            float tempMax = -Mathf.Infinity;
            for (int i = 0; i < dataRanges.Length; i++)
            {
                DataRange dr = dataRanges[i];

                if (dr.min < tempMin)
                {
                    tempMin = dr.min;
                }

                if (dr.max > tempMax)
                {
                    tempMax = dr.max;
                }
            }

            return new List<float> {tempMin, tempMax};
        }

        public void GenerateColormap()
        {
            colors = new Dictionary<Color, DataRange>();
            XmlTextReader colormapReader;

            if (useOnline)
            {
                Debug.Log("Generating colormap: " + url);
                colormapReader = new XmlTextReader(url);
            }
            else
            {
                string identifier = Path.GetFileNameWithoutExtension(url);
//			Debug.Log ("Generating colormap: " + identifier);
                TextAsset colormapAsset = Resources.Load(colormapBasePath + "/" + identifier) as TextAsset;
                if (colormapAsset != null && colormapAsset.text != null && !colormapAsset.text.Equals(""))
                {
                    colormapReader = new XmlTextReader(new StringReader(colormapAsset.text));
                }
                else
                {
                    Debug.LogError("Could not load colormap for layer: " + identifier);
                    return;
                }

            }

            while (colormapReader.Read())
            {
                if (colormapReader.NodeType == XmlNodeType.Element)
                {
                    if (colormapReader.Name.Equals("ColorMap") && colormapReader.GetAttribute("title") != "No Data")
                    {
                        title = colormapReader.GetAttribute("title");
                        units = colormapReader.GetAttribute("units");
                        while (colormapReader.Read() &&
                               !(colormapReader.NodeType == XmlNodeType.EndElement &&
                                 colormapReader.Name.Equals("ColorMap")))
                        {
                            if (colormapReader.Name.Equals("ColorMapEntry"))
                            {
                                bool transparency = Convert.ToBoolean(colormapReader.GetAttribute("transparent"));
                                string valString = colormapReader.GetAttribute("value");
                                DataRange range = ParseRange(valString, transparency);
                                string colorString = colormapReader.GetAttribute("rgb");
                                System.Diagnostics.Debug.Assert(colorString != null, "colorString != null");
                                string[] colorsStringArray = colorString.Split(',');
                                Color colorNode =
                                    new Color(Convert.ToSingle(colorsStringArray[0]) / 255,
                                        Convert.ToSingle(colorsStringArray[1]) / 255,
                                        Convert.ToSingle(colorsStringArray[2]) / 255);

                                colors[colorNode] = range;
                            }
                        }
                    }
                }
            }
        }

        public DataRange GetRange(Color colorInput)
        {
            if (colors.ContainsKey(colorInput))
            {
                return colors[colorInput];
            }
            //Debug.Log("color of " + colorInput + " not found in color map at " + url);
            throw new KeyNotFoundException("Color not specified in color map");
        }

        private static DataRange ParseRange(string valString, bool transparent = false)
        {
            DataRange newRange = new DataRange();
            Regex rangeRegex = new Regex(@"(\[|\()(\d+(\.\d+)*(\,\d+(\.\d+)*)?)(\]|\))+");
            Match matches = rangeRegex.Match(valString);

            if (matches.Success)
            {
                string currentRangeString = matches.Groups[2].ToString();

                if (currentRangeString.Contains(","))
                {
                    //Debug.Log("IF Parsing range for " + valString);

                    string[] rangesString = currentRangeString.Split(',');
                    newRange.min = Convert.ToSingle(rangesString[0]);
                    newRange.max = Convert.ToSingle(rangesString[1]);

                    //Debug.Log("Obtained value0 of " + rangesString[0]);
                    //Debug.Log("Obtained value1 of " + rangesString[1]);
                    //Debug.Log("======");

                }
                else
                {
                    float singleVal = Convert.ToSingle(currentRangeString);
                    //Debug.Log("ELSE Parsing range for " + valString);
                    //Debug.Log("Obtained single value of " + singleVal);
                    //Debug.Log("======");
                    newRange.min = singleVal;
                    newRange.max = singleVal;
                }
                //Debug.Log (newRange.min + " - "+newRange.max);

            }

            newRange.transparent = transparent;
            return newRange;
        }
    }

}