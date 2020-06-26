using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script should be attached to the sphere upon which you would like to plot your points
public class SphereLinePlot : MonoBehaviour
{
    private const float EARTH_RADIUS = 6371000; // Earth's radius in meters, used in Coordinates.LatLongToXYZ()
    private const float LINE_THICKNESS = 0.005f;
    private List<GameObject> linePlots; // List of all line plots we have created
    private float radius;
    private Material lineMat;

    void Awake()
    {
        radius = transform.localScale.x / 2;
    }


    // Plots data in a line plot.
    // `points` is an array of Vector3's
    // points[i].x is latitude
    // points[i].y is longitude
    // points[i].z is height/elevation
    // `elevationScale` will be used in determining XYZ world coordinates, defaults to 10 if not specified
    public void Plot(Vector3[] points, float elevationScale = 1000)
    {
        LineRenderer line = CreateLine(points.Length);

        // Create a new array to hold our newly converted points
        Vector3[] xyzPoints = new Vector3[points.Length];
        for (int i = 0; i < xyzPoints.Length; i++)
        {
            // Convert from latitude and longitude and elevation to XYZ world coordinates
            xyzPoints[i] = Coordinates.LatLongToXYZ(points[i].x, points[i].y, points[i].z, EARTH_RADIUS, radius, elevationScale);
        }
        line.SetPositions(xyzPoints);
    }


    // Overload that Plots data in a line plot as-is.
    // Doesn't convert from lat/long to global XYZ coordinates
    // `points` is an array of Vector3's
    // points[i].x is x-coordinate
    // points[i].y is y-coordinate
    // points[i].z is z-coordinate
    public void Plot(Vector3[] points)
    {
        LineRenderer line = CreateLine(points.Length);
        line.SetPositions(points);
    }


    // Creates and returns a LineRender attached to a new GameObject
    private LineRenderer CreateLine(int numPoints)
    {
        // Do we need to initialize?
        // This will occur the first time we plot and the next time we plot after removing all.
        if(linePlots == null)
        {
            linePlots = new List<GameObject>();
            lineMat = new Material(Shader.Find("Sprites/Default"));
            lineMat.color = Color.red;
        }

        // Create new gameObject, make it a child of the sphere
        GameObject linePlotObj = new GameObject("Line Plot " + linePlots.Count);
        linePlotObj.transform.parent = gameObject.transform;

        // Add a LineRenderer component to our child object so that we can render some lines.
        LineRenderer line = linePlotObj.AddComponent(typeof(LineRenderer)) as LineRenderer;
        line.material = lineMat;
        line.startWidth = LINE_THICKNESS;
        line.endWidth = LINE_THICKNESS;

        // The line should be as long as the number of points we have.
        line.positionCount = numPoints;
        return line;
    }


    // Cleans up and removes all the line plots it created
    public void RemovePlots()
    {
        // Destroy the material we created
        Destroy(lineMat);
        // Destroy all our line plots
        int lineCount = linePlots.Count;
        for (int i = 0; i < lineCount; i ++)
		{
			GameObject condemned = linePlots[0];
			linePlots.RemoveAt(0);
			Destroy(condemned);
		}
    }

    // Make sure we clean up while we still can
    private void OnDisable()
    {
        RemovePlots();
    }

}
