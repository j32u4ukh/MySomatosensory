using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReadCsvTest : MonoBehaviour
{
    string path;

    // Start is called before the first frame update
    void Start()
    {
        path = Path.Combine(Application.streamingAssetsPath, "threshold_config.csv");
        print("path:" + path);
        string line;

        using (StreamReader reader = new StreamReader(path))
        {
            while((line = reader.ReadLine()) != null)
            {
                var row = line.Split(',');
                var _type = row[0];

                print("");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
