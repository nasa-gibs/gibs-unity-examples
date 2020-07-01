using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

//data reader for extracting x, y, z, and val data from a csv file
public class CSVParser
{
    public static SerialData AirsReader()
    {
        string path = Application.dataPath + "/Data/output/airs.csv";
        SerialData airsData = new SerialData();
        string[] airsOutput = File.ReadAllLines(path);
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

            airsData.x.Add(float.Parse(items[0]));
            airsData.y.Add(float.Parse(items[1]));
            airsData.z.Add(float.Parse(items[2]));
            airsData.val.Add(float.Parse(items[3]));
        }
        return airsData;
    }

}
