using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Instantiates spheres to represent AIRS data points 

public class AirsRender : MonoBehaviour
{
    public GameObject[] dataPoint;

    // Start is called before the first frame update
    void Start()
    {
        SerialData airsData = CSVParser.AirsReader();
        int dataLength = airsData.x.Count;
        dataPoint = new GameObject[dataLength];

        //Converts List<float> of AIRS coordinates into Vector3 object, instantiates data points accordingly 
        for (int i = 0; i > dataLength; i++)
        {
            dataPoint[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 airsCords = Coordinates.XYZToLatLong(airsData.x[i], airsData.y[i], airsData.z[i]);
            Instantiate(dataPoint[i], airsCords, Quaternion.identity);
        }
    }
}
