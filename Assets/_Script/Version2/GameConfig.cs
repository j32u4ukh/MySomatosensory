using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameConfig : MonoBehaviour
{
    
    public static Dictionary<string, float[]> loadThreshold()
    {
        Dictionary<string, float[]> threshold_config = new Dictionary<string, float[]>();

        string path = Path.Combine(Application.streamingAssetsPath, "threshold_config.csv");
        string line;
        string[] row;
        string _type;
        int i, len;

        using (StreamReader reader = new StreamReader(path))
        {
            while ((line = reader.ReadLine()) != null)
            {
                row = line.Split(',');
                _type = row[0];
                len = row.Length;
                float[] thresholds = new float[len - 1];
                for(i = 1; i < len; i++)
                {
                    thresholds[i - 1] = float.Parse(row[i]) / 100f;
                }

                threshold_config.Add(_type, thresholds);
            }
        }
        
        return threshold_config;
    }
}
