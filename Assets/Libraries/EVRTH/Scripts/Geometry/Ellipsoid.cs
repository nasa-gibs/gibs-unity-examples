using System;
using EVRTH.Scripts.WMS;
using UnityEngine;

namespace EVRTH.Scripts.Geometry
{
    /// <summary>
    /// Geometric representation of an Ellipsoid.
    /// </summary>
    public class Ellipsoid
    {
        public static readonly Ellipsoid Wgs84 = new Ellipsoid(6378137.0f, 6378137.0f, 6356752.314245f);
        public static readonly Ellipsoid ScaledWgs84 = new Ellipsoid(1.0f, 1.0f, 6356752.314245f / 6378137.0f);

        private readonly Vector3 radii;
        private Vector3 oneOverRadiiSquared;

        public Vector3 Radii
        {
            get { return radii; }
        }

        public Ellipsoid(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        public Ellipsoid(Vector3 radii)
        {
            if ((radii.x <= 0.0) || (radii.y <= 0.0) || (radii.z <= 0.0))
            {
                throw new ArgumentOutOfRangeException("radii");
            }

            this.radii = radii;

            oneOverRadiiSquared = new Vector3(
                1.0f / (radii.x * radii.x),
                1.0f / (radii.y * radii.y),
                1.0f / (radii.z * radii.z));
        }

        /// <summary>
        /// Compute the geodetic surface normal at a point on the ellipsoid.
        /// </summary>
        /// <param name="positionOnEllipsoid">A position on the WGS84 
        /// coordinate frame that lies on the ellipsoid.</param>
        /// <returns>The surface normal for the given position.</returns>
        public Vector3 GeodeticSurfaceNormal(Vector3 positionOnEllipsoid)
        {
            // Compute surface normal using equation from section 2.2.2 of "3D Engine Design for Virtual Globes"
            return new Vector3(
                positionOnEllipsoid.x * oneOverRadiiSquared.x,
                positionOnEllipsoid.y * oneOverRadiiSquared.y,
                positionOnEllipsoid.z * oneOverRadiiSquared.z).normalized;
        }

        /// <summary>
        /// Compute the geodetic surface normal at a geographic location.
        /// </summary>
        /// <param name="latlon">A position in the WGS84 geographic
        /// coordinate system.</param> 
        /// <returns>The surface normal for the given position.</returns>
        public Vector3 GeodeticSurfaceNormal(LatLon latlon)
        {
            float cosLatitude = Mathf.Cos(latlon.Latitude);

            return new Vector3(
                cosLatitude * Mathf.Cos(latlon.Longitude),
                cosLatitude * Mathf.Sin(latlon.Longitude),
                Mathf.Sin(latlon.Latitude));
        }
    }
}
