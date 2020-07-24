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

        //Reads data passed from instantiator script, validates if file was read correctly
        if (File.Exists(path))
        {
            Debug.Log(path);
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

                // Checks to make sure that the object was parsed correctly
                if (data.x == null || data.y == null || data.z == null || data.val == null)
                {
                    Debug.Log("Data object is null");
                }
            }
        }
        else
        {
            Debug.Log("Cannot find file in " + path);
        }

        return data;
    }
}
