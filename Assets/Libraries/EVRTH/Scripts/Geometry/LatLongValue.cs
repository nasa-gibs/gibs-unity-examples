using UnityEngine;

// A simple struct use to pair up lat/long combinations with their respective scientific value,
// used to store this data in order to do statistical analysis before visualizing it.
namespace EVRTH.Scripts.Geometry
{
    public struct LatLongValue
    {
        public float latitude;
        public float longitude;
        public float scientificValue;

        public LatLongValue(float lat, float lon, float val)
        {
            latitude = lat;
            longitude = lon;
            scientificValue = val;
        }

        public override string ToString()
        {
            return string.Format("({0,3:N2}, {1,3:N2})\nValue: {2,6:N3} ppb", latitude, longitude, scientificValue);
        }

        public static LatLongValue GetLatLonFrom3DPoint(Vector3 point, Transform planetTransform)
        {
            LatLongValue latLon = new LatLongValue();
            Vector3 normalizedVectorToPoint = (point - planetTransform.position).normalized;
            Vector3 vectorInEarthSpace = planetTransform.InverseTransformDirection(normalizedVectorToPoint);

            latLon.latitude = Mathf.Asin(vectorInEarthSpace.y) * Mathf.Rad2Deg;
            float sign = Mathf.Sign(-Mathf.Asin(vectorInEarthSpace.x / Mathf.Cos(latLon.latitude * Mathf.Deg2Rad)) * Mathf.Rad2Deg);
            latLon.longitude = sign * Mathf.Acos(Mathf.Clamp(vectorInEarthSpace.z / Mathf.Cos(latLon.latitude * Mathf.Deg2Rad), -1.0f, 1.0f)) * Mathf.Rad2Deg;

            return latLon;
        }

        public static Vector3 Get3DPoint(LatLongValue latLon, float sphereRadius, float heightMultiplier, float valueOffset, bool allowNegativeRadiiExtensions)
        {
            float radius;
            if (allowNegativeRadiiExtensions)
            {
                radius = sphereRadius + (heightMultiplier * (latLon.scientificValue + valueOffset));
            }
            else
            {
                radius = sphereRadius + Mathf.Clamp(heightMultiplier * (latLon.scientificValue + valueOffset), 0.0f, Mathf.Infinity);
            }
        
            if ((-90.0f <= latLon.latitude) && (latLon.latitude <= 90.0f) && (-180.0f <= latLon.longitude) && (latLon.longitude <= 180.0f))
            {
                // We're assuming that z+ points towards the intersection of the prime meridian/equator
                float z = radius * Mathf.Cos(latLon.longitude * Mathf.Deg2Rad) * Mathf.Cos(latLon.latitude * Mathf.Deg2Rad);
                float x = -radius * Mathf.Sin(latLon.longitude * Mathf.Deg2Rad) * Mathf.Cos(latLon.latitude * Mathf.Deg2Rad);
                float y = radius * Mathf.Sin(latLon.latitude * Mathf.Deg2Rad);

                return new Vector3(x, y, z);
            }
            Debug.LogError("Bad lat/long values given!");
            return new Vector3(0.0f, 0.0f, 0.0f);
        }
    }
}