using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Instantiates spheres to represent AIRS data points 

public class AirsRender : MonoBehaviour
{
    public GameObject gibsObject;
    public GameObject[] dataPoint;


    // Start is called before the first frame update
    void Start()
    {
        SerialData airsData = CSVParser.AirsReader();
        int dataLength = airsData.x.Count;
        dataPoint = new GameObject[dataLength];

        //Converts List<float> of AIRS coordinates into Vector3 object, instantiates data points accordingly 
        for (int i = 0; i < dataLength; i++)
        {
            dataPoint[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dataPoint[i].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            var airsRenderer = dataPoint[i].GetComponent<Renderer>();
            float colorValue = airsData.val[i];
            
            float sphereRadius = GetComponent<SphereCollider>().radius;
            float actualRadius = 3948.8F;
            float elevationScale = .70F;

           //WIP Datapoint Color System
            if(colorValue > 0.0F && colorValue < 10.0F)
                airsRenderer.material.SetColor("_Color", Color.red);
            if(colorValue > 10.0F && colorValue < 20.0F)
                airsRenderer.material.SetColor("_Color", Color.magenta);
            if (colorValue > 20.0F && colorValue < 30.0F)
                airsRenderer.material.SetColor("_Color", Color.yellow);
            if (colorValue > 30.0F && colorValue < 40.0F)
                airsRenderer.material.SetColor("_Color", Color.green);
            if (colorValue > 40.0F && colorValue < 50.0F)
                airsRenderer.material.SetColor("_Color", Color.cyan);
            if (colorValue > 50.0F && colorValue < 60.0F)
                airsRenderer.material.SetColor("_Color", Color.blue);
            if (colorValue > 60.0F && colorValue < 70.0F)
                airsRenderer.material.SetColor("_Color", Color.black);
            if (colorValue > 70.0F && colorValue < 80.0F)
                airsRenderer.material.SetColor("_Color", Color.gray);
            if (colorValue > 80.0F && colorValue < 90.0F)
                airsRenderer.material.SetColor("_Color", Color.grey);
            if (colorValue > 90.0F && colorValue < 100.0F)
                airsRenderer.material.SetColor("_Color", Color.white);

            // public static Vector3 LatLongToXYZ(float latitude, float longitude, float elevation,
            //                      float actualRadius, float sphereRadius, float elevationScale)
            Vector3 airsCords = Coordinates.LatLongToXYZ(airsData.x[i], airsData.y[i], airsData.z[i], actualRadius, sphereRadius, elevationScale);
            Instantiate(dataPoint[i], transform.position + airsCords, Quaternion.identity);
            //dataPoint[i].transform.parent = gibsObject.transform;
        }
    }

}
