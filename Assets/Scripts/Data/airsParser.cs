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

        string[] plyOutput = File.ReadAllLines(path);
        // Get vertex count
        foreach (string currentLine in plyOutput)
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
        dataFile.type = airsFile.DataType.pointcloud;
        if (dataFile.identifier == null || dataFile.identifier.Trim() == "")
            dataFile.identifier = Path.GetFileName(path);
        Debug.Log(dataFile.identifier);
        return dataFile;
    }

    public static airsFile ReadModelFromPath(string path, airsFile dataFile, DataObject dataObject, float loadWeight)
    {
        airsFile data = new airsFile();
        string[] plyOutput = File.ReadAllLines(path);
        if (dataFile == null)
            dataFile = MetadataFromPath(path);

        //Assign from DataFile
        string variable = dataFile.identifier.Substring(dataFile.identifier.IndexOf('_') + 1);

        data.fileName = dataFile.fileName;

        data.x = dataFile.x;
        data.y = dataFile.y;
        data.z = dataFile.z;
        data.val = dataFile.val;

        int line = 0;
        bool headerFlag = false;
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> z = new List<float>();
        List<float> val = new List<float>();
        foreach (string currentLine in plyOutput)
        {
            string[] items = currentLine.Split(' ');

            // Skip header
            if (!headerFlag)
            {
                if (items[0] == "end_header")
                {
                    headerFlag = true;
                }
                continue;
            }

            x.Add(float.Parse(items[0]));
            y.Add(float.Parse(items[1]));
            z.Add(float.Parse(items[2]));
            val.Add(float.Parse(items[3]));

            if (line % 1000 == 0 || line == plyOutput.Length - 1)
            {
                ThreadManager.instance.callbacks.Add(() => {
                    dataObject.UpdateLoadPercent((float)line / plyOutput.Length * loadWeight, "Parsing File");
                });
            }
            line++;

        }

        data.xIndexDefault = "mesh.x";
        data.yIndexDefault = "mesh.y";
        data.zIndexDefault = "mesh.z";
        data.colorIndexDefault = variable;

        data.vars[data.xIndexDefault] = x.ToArray();
        data.vars[data.yIndexDefault] = y.ToArray();
        data.vars[data.zIndexDefault] = z.ToArray();
        data.vars[data.colorIndexDefault] = val.ToArray();

        return data;
    }

    private static string ToTitleCase(string str)
    {
        if (str != "")
            return char.ToUpper(str[0]) + ((str.Length > 1) ? str.Substring(1) : string.Empty);
        return string.Empty;
    }

}
