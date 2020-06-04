using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using Random = UnityEngine.Random;

public class Data
{
    public string id;
    public Vector3 pos;

    public Data(Vector3 vector3)
    {
        id = Guid.NewGuid().ToString();
        pos = vector3;
    }
}

public class JsonTest : MonoBehaviour {
    StreamWriter writer;
    StreamReader reader;
    Data data;
    string path;

    // Use this for initialization
    void Start () {        
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "data_test.txt");
        outputData(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
        if (data == null)
        {
            Debug.Log("data is null");
            loadData();
        }
        Debug.Log(string.Format("guid:{0}\nposition:({1}, {2}, {3})",
            data.id, data.pos.x, data.pos.y, data.pos.z));
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void outputData(Vector3 vector3)
    {
        data = new Data(vector3);

        // 檢查檔案是否存在，不存在則建立
        if (!File.Exists(path))
        {
            writer = new FileInfo(path).CreateText();
        }
        else
        {
            writer = new FileInfo(path).AppendText();
        }

        writer.WriteLine(JsonConvert.SerializeObject(data));
        Debug.Log("write out the data");
        writer.Close();
        writer.Dispose();
        data = null;
    }

    void loadData()
    {
        reader = new StreamReader(path);
        string load_data = reader.ReadToEnd().Trim();
        reader.Close();

        data = JsonConvert.DeserializeObject<Data>(load_data);
        Debug.Log("data loaded in.");
    }
}
