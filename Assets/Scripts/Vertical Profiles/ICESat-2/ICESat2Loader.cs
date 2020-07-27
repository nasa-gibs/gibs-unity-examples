using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ICESat2Loader : MonoBehaviour
{
    public SphereLinePlot linePlot;

    // Start is called before the first frame update
    void Start()
    {
        //Loads ICESat-2 geojson x, y, z values into C# data object
        string path = Application.dataPath + "/Resources/Data/output/icesat2.geojson";
        GeojsonData iceData = GeojsonParser.Reader(path);
        Debug.Log("x: " + iceData.features[0].geometry.coordinates[0] + "\ny: " + iceData.features[0].geometry.coordinates[1] + "\nz: " + iceData.features[0].properties.height);
        Vector3[] iceVector = new Vector3[iceData.features.Count];
        
        //Creates and initializes a Vector3 with values from ICESat-2 data object
        for (int i = 0; i < iceData.features.Count; i++)
        {
            iceVector[i] = new Vector3(iceData.features[i].geometry.coordinates[0], iceData.features[i].geometry.coordinates[1], iceData.features[i].properties.height);
        }

        Debug.Log(iceVector[1]);

        linePlot.Plot(iceVector);
    }
}
