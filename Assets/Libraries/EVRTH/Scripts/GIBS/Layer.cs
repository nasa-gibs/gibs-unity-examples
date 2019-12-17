using System;
using System.Collections.Generic;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.GIBS
{
    public class Layer
    {
        // Config Props
        //"http://gibs.earthdata.nasa.gov/wmts/epsg4326/best/{Layer}/default/{Time}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}.{Extension}";
        private const string wmtsTemplate = "http://gibs.earthdata.nasa.gov/wmts/epsg4326/best/{0}/default/{1}/{2}/{3}/{4}/{5}.{6}";
        private const string wmsTemplate = "https://gibs-a.earthdata.nasa.gov/wms/wms.php?LAYERS={0}&TIME={1}&SERVICE=WMS&VERSION=1.1.1&REQUEST=GetMap&FORMAT=image%2Fpng&TRANSPARENT=true&WIDTH=512&HEIGHT=512&SRS=EPSG%3A4326&STYLES=&BBOX={2}";

        // Properties
        public string identifier;
        public string title;
        public string format;
        public string tileMatrixSetIdentifier;
        public bool useOnline = false;
        public bool isWms = false;

        //public DateTime startTime, endTime;
        public LatLonBoundingBox bbox;
        public string colormapUrl;

        public List<TileMatrix> tileMatrixSet;
        private ColorMap colormap;
        public ColorMap Colormap
        {
            get { return colormap ?? (colormap = new ColorMap(colormapUrl)); }
            set
            {
                colormap = value;
            }
        }

        public int MaxDepth
        {
            get
            {
                return tileMatrixSet.Count - 1;

            }
        }

        //Conversion values
        private const decimal units = 111319.490793274M; // meters/degree
        private const int tilesize = 512; // pixels
        private const decimal pixelsize = 0.00028M; // meters
        private const decimal topLeftMinx = -180;
        private const decimal topLeftMaxy = 90;

        // Contructor

        // Methods

        // Manually force colormap load
        public void LoadColorMap()
        {
            if (colormapUrl != null)
            {
                colormap = new ColorMap(colormapUrl, useOnline);
            }
        }

        private decimal Truncate(decimal value)
        {
            return decimal.Truncate(1000 * value) / 1000;
        }

        public Wmts BBox2Wmts(LatLonBoundingBox bboxTemp)
        {
            // calculate additional top_left values for reference
            //decimal topLeftMaxx = topLeftMinx + (int)bboxTemp.DeltaLon;
            decimal topLeftMiny = topLeftMaxy - (int)bboxTemp.DeltaLat;

            // calculate col and row
            decimal col = Math.Abs(((decimal)bboxTemp.minLon - topLeftMinx) / (int)bboxTemp.DeltaLon);
            decimal row = Math.Abs(((decimal)bboxTemp.minLat - topLeftMiny) / (int)bboxTemp.DeltaLat);

            // calculate scale denominator for reference
            decimal scaleDenominator = (decimal)bboxTemp.DeltaLon * 2 / pixelsize * units / (tilesize * 2);
            TileMatrix currentMatrix = tileMatrixSet.Find(item => Truncate(item.scaleDenominator) == Truncate(scaleDenominator));
            int zoom = int.Parse(currentMatrix.identifier);

            Wmts request = new Wmts { row = (int)row, col = (int)col, zoom = zoom };
            return request;
        }

        public LatLonBoundingBox Wmts2Bbox(int row, int col, int zoom)
        {
            return Wmts2Bbox(new Wmts { row = row, col = col, zoom = zoom });
        }

        public LatLonBoundingBox Wmts2Bbox(Wmts request)
        {
            TileMatrix currentMatrix = tileMatrixSet[request.zoom];
            decimal scaleDenominator = currentMatrix.scaleDenominator;

            decimal size = tilesize * 2 * scaleDenominator / units * (pixelsize / 2);

            //decimal topLeftMaxx = -180 + size;
            //decimal topLeftMiny = 90 - size;

            decimal requestMinx = topLeftMinx + request.col * size;
            decimal requestMiny = topLeftMaxy - request.row * size - size;
            decimal requestMaxx = topLeftMinx + request.col * size + size;
            decimal requestMaxy = topLeftMaxy - request.row * size;

            return new LatLonBoundingBox((float) requestMiny, (float) requestMaxy, (float) requestMinx,
                (float) requestMaxx);
        }

        public static LatLonBoundingBox Wmts2DefaultBbox(Wmts request)
        {
            decimal scaleDenominator = TileMatrix.defaultTileMatrix.scaleDenominator / (decimal)Math.Pow(2.0, request.zoom - 1);

            decimal size = tilesize * 2 * scaleDenominator / units * (pixelsize / 2);

            //decimal topLeftMaxx = -180 + size;
            //decimal topLeftMiny = 90 - size;

            decimal requestMinx = topLeftMinx + request.col * size;
            decimal requestMiny = topLeftMaxy - request.row * size - size;
            decimal requestMaxx = topLeftMinx + request.col * size + size;
            decimal requestMaxy = topLeftMaxy - request.row * size;

            LatLonBoundingBox bbox = new LatLonBoundingBox((float)requestMiny, (float)requestMaxy, (float)requestMinx, (float)requestMaxx);
            return bbox;
        }

        public string WmtsToUrl(int row, int col, int zoom, DateTime time)
        {
            string finalUrl;

            string timeString = time.ToString("yyyy-MM-dd");
            // If we are beyond the supported zoom level, we must gate the row and column back accordingly
            if (zoom > MaxDepth)
            {
                int zoomDifference = Mathf.FloorToInt(Mathf.Pow(2.0f, zoom - MaxDepth));
                zoom = MaxDepth;
                row = row / zoomDifference;
                col = col / zoomDifference;
            }

            if (!isWms)
            {
                string extension = null;
                switch (format)
                {
                    case "image/jpeg":
                        extension = "jpg";
                        break;
                    case "image/png":
                        extension = "png";
                        break;
                }
                finalUrl = string.Format(wmtsTemplate, identifier, timeString, tileMatrixSetIdentifier, zoom, row, col, extension);

            }
            else // Fire should be this
            {
                LatLonBoundingBox bboxTemp = Wmts2Bbox(row, col, zoom);
                String bboxString = bboxTemp.minLon + "," + bboxTemp.minLat + "," + bboxTemp.maxLon + "," + bboxTemp.maxLat;
                finalUrl = string.Format(wmsTemplate, identifier, timeString, bboxString);
            }


            return finalUrl;
        }

        public string WmtsToUrl(Wmts request, DateTime time)
        {
            return WmtsToUrl(request.row, request.col, request.zoom, time);
        }

        public override string ToString()
        {
            return identifier;
        }
    }
}
