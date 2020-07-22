using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

//Data reader for parsing x, y, z, and val data from a csv file
public class CSVParser
{
    public static CSVData Reader(string path)
    {
        CSVData data = new CSVData();
        string[] csvString = File.ReadAllLines(path);
        bool headerFlag = true;
        
        foreach (string currentLine in csvString)
        {
            string[] items = currentLine.Split(',');

            // Skips header
            if (headerFlag)
            {
                headerFlag = false;
                continue;
            }

            data.x.Add(float.Parse(items[0]));
            data.y.Add(float.Parse(items[1]));
            data.z.Add(float.Parse(items[2]));
            data.val.Add(float.Parse(items[3]));
        }
        return data;
    }

}
