using System.Collections.Generic;
using UnityEngine;

namespace EVRTH.Scripts.WMS
{
    public class LatLonBoundingBox
    {
        public readonly float minLat;
        public readonly float maxLat;
        public readonly float minLon;
        public readonly float maxLon;

        public float DeltaLat
        {
            get
            {
                return maxLat - minLat;
            }
        }
        public float DeltaLon
        {
            get
            {
                return maxLon - minLon;
            }
        }

        public LatLon Min
        {
            get
            {
                return new LatLon(minLat, minLon);
            }
        }
        public LatLon Max
        {
            get
            {
                return new LatLon(maxLat, maxLon);
            }
        }

        public LatLon Center
        {
            get
            {
                return new LatLon((minLat + maxLat) * 0.5f,
                    (minLon + maxLon) * 0.5f);
            }
        }

        public LatLonBoundingBox(float minLat, float maxLat, float minLon, float maxLon)
        {
            this.minLat = minLat;
            this.maxLat = maxLat;
            this.minLon = minLon;
            this.maxLon = maxLon;
        }

        public LatLonBoundingBox Clamp()
        {
            return new LatLonBoundingBox(
                Mathf.Clamp(minLat, -90, 90),
                Mathf.Clamp(maxLat, -90, 90),
                Mathf.Clamp(minLon, -180, 180),
                Mathf.Clamp(maxLon, -180, 180));
        }

        public IEnumerable<LatLonBoundingBox> Subdivide()
        {
            float avgLat = (minLat + maxLat) * 0.5f;
            float avgLon = (minLon + maxLon) * 0.5f;

            // -------------
            // |  2  |  3  |
            // |-----|-----|
            // |  1  |  4  |
            // -------------
            yield return new LatLonBoundingBox(minLat, avgLat, minLon, avgLon);
            yield return new LatLonBoundingBox(avgLat, maxLat, minLon, avgLon);
            yield return new LatLonBoundingBox(avgLat, maxLat, avgLon, maxLon);
            yield return new LatLonBoundingBox(minLat, avgLat, avgLon, maxLon);
        }

        public bool Contains(LatLonBoundingBox otherBox)
        {
            return (minLat <= otherBox.minLat && maxLat >= otherBox.maxLat && minLon <= otherBox.minLon && maxLon >= otherBox.maxLon);
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})-({2}, {3})", minLat, minLon, maxLat, maxLon);
        }
    }
}
