
namespace EVRTH.Scripts.WMS
{
    public struct LatLon
    {
        public float Latitude { get; set; } // TODO should validate angles
        public float Longitude { get; set; }

        public LatLon(float lat, float lon) : this()
        {
            Latitude = lat;
            Longitude = lon;
        }
    }
}
