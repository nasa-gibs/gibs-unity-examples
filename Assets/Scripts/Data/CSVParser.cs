using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

public class CSVParser
{
    public static SerialData airsReader(string path)
    {
        SerialData airsData = new SerialData();
        string[] airsOutput = File.ReadAllLines(path);
        bool headerFlag = true;
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> z = new List<float>();
        List<float> val = new List<float>();
        
        foreach (string currentLine in airsOutput)
        {
            string[] items = currentLine.Split(',');

            // Skips header
            if (headerFlag)
            {
                headerFlag = false;
                continue;
            }

            x.Add(float.Parse(items[0]));
            y.Add(float.Parse(items[1]));
            z.Add(float.Parse(items[2]));
            val.Add(float.Parse(items[3]));
        }
        return airsData;
    }
    public static SerialData CalipsoReader(string path)
    {
        SerialData calipsoData = new SerialData();
        string[] calipsoOutput = File.ReadAllLines(path);
        bool headerFlag = true;
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> z = new List<float>();
        List<float> val = new List<float>();

        foreach (string currentLine in calipsoOutput)
        {
            string[] items = currentLine.Split(',');

            // Skips header
            if (headerFlag)
            {
                headerFlag = false;
                continue;
            }

            x.Add(float.Parse(items[0]));
            y.Add(float.Parse(items[1]));
            z.Add(float.Parse(items[2]));
            val.Add(float.Parse(items[3]));
        }
        return calipsoData;
    }
}
