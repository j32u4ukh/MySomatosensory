using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StructTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StructData data = StructData.load();
        print(data.struct1.ToString());

        foreach(var info in data.structs)
        {
            print(info.ToString());
        }
        //StructInfo info1 = new StructInfo() {
        //    name = "one",
        //    vector3_dict = new Dictionary<int, Vector3>() {
        //        { 1, new Vector3(1f, 2f, 3f)},
        //        { 2, new Vector3(4f, 5f, 6f)},
        //        { 3, new Vector3(7f, 8f, 9f)},
        //    }
        //};
        //data.struct1 = info1;

        //StructInfo info2 = new StructInfo()
        //{
        //    name = "two",
        //    vector3_dict = new Dictionary<int, Vector3>() {
        //        { 4, new Vector3(2f, 4f, 6f)},
        //        { 5, new Vector3(8f, 10f, 12f)},
        //        { 6, new Vector3(14f, 16f, 18f)},
        //    }
        //};

        //StructInfo info3 = new StructInfo()
        //{
        //    name = "three",
        //    vector3_dict = new Dictionary<int, Vector3>() {
        //        { 7, new Vector3(3f, 6f, 9f)},
        //        { 8, new Vector3(12f, 15f, 18f)},
        //        { 9, new Vector3(21f, 24f, 27f)},
        //    }
        //};

        //List<StructInfo> structs = new List<StructInfo> { info2 , info3 };
        //data.structs = structs;

        //data.save();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public struct StructInfo
{
    public string name;
    public Dictionary<int, Vector3> vector3_dict;
    public override string ToString() {
        string output = string.Format("name: {0}", name);

        foreach(var key in vector3_dict.Keys)
        {
            output = string.Format("{0}\n{1}: {2}", output, key, vector3_dict[key]);
        }

        return output;
    }
}

public class StructData
{
    public StructInfo struct1;
    public List<StructInfo> structs;

    public void save()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "StructTest.txt");

        // 檢查檔案是否存在，不存在則建立
        StreamWriter writer = new FileInfo(path).CreateText();

        // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
        writer.WriteLine(JsonConvert.SerializeObject(this));
        writer.Close();
        writer.Dispose();
    }

    public static StructData load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "StructTest.txt");

        StreamReader reader = new StreamReader(path);
        string load_data = reader.ReadToEnd().Trim();
        reader.Close();
        return JsonConvert.DeserializeObject<StructData>(load_data);
    }
}
