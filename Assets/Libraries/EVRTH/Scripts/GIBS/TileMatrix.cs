using EVRTH.Scripts.WMS;

namespace EVRTH.Scripts.GIBS
{
    public struct TileMatrix
    {

        public string identifier;
        public decimal scaleDenominator;
        public LatLon topLeftCorner;
        public int tileWidth, tileHeight, matrixWidth, matrixHeight;

        public override string ToString()
        {
            return identifier + " - scaleDenominator: " + scaleDenominator + " - matrixDimensions: " + matrixWidth + "x" + matrixHeight;
        }

        public static TileMatrix defaultTileMatrix = new TileMatrix
        {
            identifier = "1",
            matrixWidth = 3,
            matrixHeight = 2,
            tileWidth = 512,
            tileHeight = 512,
            scaleDenominator = 111816452.8057436m,
            topLeftCorner = new LatLon { Latitude = -180.0f, Longitude = 90.0f }
        };
    }
}
