using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Debug = UnityEngine.Debug;

public class airsParser
{
    public static airsFile MetadataFromPath(string path)
    {
        airsFile dataFile = new airsFile();

        dataFile.fileName = Path.GetFileName(path);
        dataFile.path = path;

        string[] airsOutput = File.ReadAllLines(path);
        // Get AIRS values
        foreach (string currentLine in airsOutput)
        {
            string[] items = currentLine.Split(null, 3);
            if (items != null && items.Length > 0)
            {
                switch (items[0])
                {
                    case "comment":
                        if (items[1] == "x")
                            dataFile.x = float.Parse(items[2]);
                        if (items[1] == "y")
                            dataFile.y = float.Parse(items[2]);
                        if (items[1] == "z")
                            dataFile.z = float.Parse(items[2]);
                        if (items[1] == "val")
                            dataFile.val = float.Parse(items[2]);
                        break;
                }
            }
            if (items[0] == "end_header")
                break;
        }
        return dataFile;
    }
}
