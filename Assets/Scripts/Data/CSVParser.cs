using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

public class CSVParser
{
    //data reader for extracting x, y, z, and val data from a csv file
    public static SerialData AirsReader()
    {
        SerialData airsData = new SerialData();
        string[] airsOutput = File.ReadAllLines(Application.dataPath + "/airs.csv");
        bool headerFlag = true;
        
        foreach (string currentLine in airsOutput)
        {
            string[] items = currentLine.Split(',');

            // Skips header
            if (headerFlag)
            {
                headerFlag = false;
                continue;
            }

            airsData.longitude.Add(float.Parse(items[0]));
            airsData.latitude.Add(float.Parse(items[1]));
            airsData.sphereRadius.Add(float.Parse(items[2]));
            airsData.val.Add(float.Parse(items[3]));
        }
        return airsData;
    }

}
