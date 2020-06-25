using UnityEngine;

public static class Coordinates {

	// Convert latitude and longitude to XYZ world coordinates.
	// Assume sphere is located at the origin.
	public static Vector3 LatLongToXYZ(float latitude, float longitude, float sphereRadius)
	{
		// Use LatLongToSpherical to first get our spherical coordinates
		Vector3 spherical = LatLongToSpherical(latitude, longitude, sphereRadius);

		// radius is unchanged
		float theta = spherical.y;
		float phi = spherical.z;

		float y = sphereRadius * Mathf.Cos(theta);		
		float x = Mathf.Sqrt((sphereRadius * sphereRadius - y * y) / (Mathf.Pow(Mathf.Tan(phi), 2) + 1));
		float z = Mathf.Sqrt(Mathf.Max(sphereRadius * sphereRadius - y * y - x * x, 0));
		if (longitude < 0)
		{
			z = -z;
		}
		if (Mathf.Abs(longitude) > 90)
		{
			x = -x;
		}

		return new Vector3(x,y,z);
	}


	// Overload
	// Convert latitude, longitude, and elevation to XYZ world coordinates.
	// Assume sphere is located at the origin.
	// actualRadius is the actual radius of the Earth given in the same units as elevation.
	// sphereRadius is the radius of the sphere in Unity units.
	// elevationScale is a scaler value that is multiplied with elevation to raise the data further off the surface
	// of the Earth to make it more visible.
	public static Vector3 LatLongToXYZ(float latitude, float longitude, float elevation,
									   float actualRadius, float sphereRadius, float elevationScale)
	{
		// Expand the sphereRadius to include the elevation
		float sphereRadiusExtended = ((elevation * elevationScale + actualRadius) / actualRadius) * sphereRadius;
		// Use LatLongToSpherical to get our spherical coordinates
		Vector3 spherical = LatLongToSpherical(latitude, longitude, sphereRadiusExtended);

		float theta = spherical.y;
		float phi = spherical.z;

		float y = sphereRadiusExtended * Mathf.Cos(theta);		
		float x = Mathf.Sqrt((sphereRadiusExtended * sphereRadiusExtended - y * y) / (Mathf.Pow(Mathf.Tan(phi), 2) + 1));
		float z = Mathf.Sqrt(Mathf.Max(sphereRadiusExtended * sphereRadiusExtended - y * y - x * x, 0));
		if (longitude < 0)
		{
			z = -z;
		}
		if (Mathf.Abs(longitude) > 90)
		{
			x = -x;
		}

		return new Vector3(x,y,z);
	}


	// Convert XYZ world coordinates to latitude and longitude and radius.
	// Assume sphere is located at the origin.
	public static Vector3 XYZToLatLong(float x, float y, float z)
	{
		// Use XYZToSpherical to first get our spherical coordinates
		Vector3 spherical = XYZToSpherical(x, y, z);

		float sphereRadius = spherical.x;
		float theta = spherical.y;
		float phi = spherical.z;

		float latitude = -1 * (Mathf.Rad2Deg * theta - 90);
		float longitude = Mathf.Rad2Deg * phi;

		if (x < 0) {
			if (longitude < 0) {
				longitude = 90 - (-90 - longitude);
				// longitude = 180 + longitude;
				// wrap around: example: -10 degrees becomes 170 degrees
			} else {
				longitude = -90 - (90 - longitude);
				// longitude = -180 + longitude
			}
		}
		return new Vector3(latitude, longitude, sphereRadius);
	}

	// Overload
	// Same as above but takes a Vector3 to make things simpler for other scripts.
	public static Vector3 XYZToLatLong(Vector3 xyz)
	{
		return XYZToLatLong(xyz.x, xyz.y, xyz.z);
	}


	// Converts latitude and longitude to spherical coordinates (radius, polar angle (theta), azimuthal angle (phi))
	public static Vector3 LatLongToSpherical(float latitude, float longitude, float sphereRadius)
	{
		float phi;

		if (longitude > 90) {
			phi = Mathf.Deg2Rad * (longitude - 180);
		}
		else if (longitude < -90)
		{
			phi = Mathf.Deg2Rad * (180 + longitude);
		}
		else
		{
			phi = Mathf.Deg2Rad * longitude;
		}

		float theta = Mathf.Deg2Rad * (90 - latitude);

		return new Vector3(sphereRadius, theta, phi);
	}


	// Convert XYZ world coordinates to spherical coordinates (radius, polar angle (theta), azimuthal angle (phi))
	// Assume sphere is located at the origin.
	public static Vector3 XYZToSpherical(float x, float y, float z)
	{
		float sphereRadius = Mathf.Sqrt (x * x + y * y + z * z);
		float theta = Mathf.Acos (y / sphereRadius);
		float phi = Mathf.Atan(z / x);
		if (float.IsNaN(phi)) {
			phi = Mathf.Atan2 (z, x);
		}
		return new Vector3(sphereRadius, theta, phi);
	}
}
