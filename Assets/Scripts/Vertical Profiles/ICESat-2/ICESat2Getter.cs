using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ICESat2Getter : MonoBehaviour
{
    public SphereLinePlot sphereLinePlot;
    private Vector3[] icesat2Points;

    // Start is called before the first frame update
    void Start()
    {
        // Example data
        GetICESat2Data("2018-11-13", 31, 32, 79, 80, "gt1r", 706);
    }

    public Vector3[] GetIceSat2Points()
    {
        return icesat2Points;
    }

    public void GetICESat2Data(string date, float miny, float maxy, float minx, float maxx, string beamName, int trackId)
    {
        string uri = "https://openaltimetry.org/data/api/icesat2/atl03?";
        uri += "date=" + date;
        uri += "&miny=" + miny;
        uri += "&maxy=" + maxy;
        uri += "&minx=" + minx;
        uri += "&maxx=" + maxx;
        uri += "&beamName=" + beamName;
        uri += "&trackId=" + trackId;
        uri += "&client=" + "gibs_unity_examples";
        uri += "&outputFormat=" + "csv";
        Debug.Log("GET URL: " + uri);
        StartCoroutine(ICESat2DataRequest(uri));
    }

    private IEnumerator ICESat2DataRequest(string uri)
    {
        UnityWebRequest www = UnityWebRequest.Get(uri);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Got the data!");

            // Now parse the CSV
            // Split the CSV up by line
            string[] rows = www.downloadHandler.text.Split('\n');

            // Initialize list of ICESat-2 points
            List<Vector3> pointsList = new List<Vector3>();

            //handles large data mass, only renders 1 out of 1000 points
            for (int i = 1; i < rows.Length; i++)
            {
                if (i % 1000 == 0) // Take 1 out of every 1000 points
                {
                    string[] values = rows[i].Split(',');
                    pointsList.Add(new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3])));
                }
            }

            // Save points in our icesat2Points field
            icesat2Points = pointsList.ToArray();

            // If you would like to call another function after having gotten the data, do that here
            sphereLinePlot.Plot(icesat2Points);
        }
    }

}
