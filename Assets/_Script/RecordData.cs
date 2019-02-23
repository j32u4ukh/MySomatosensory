using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

class RecordData
{
    #region 要紀錄的數據
    public string guid;
    public string id;
    public string date;
    public string type;
    public string stage;
    public string start_time;
    public string end_time;
    public float[] threshold;
    public float[] accuracy;
    public List<Dictionary<HumanBodyBones, Vector3>> skeletons_list;
    public List<Dictionary<HumanBodyBones, Vector3>> rotations_list;
    #endregion    

    public RecordData()
    {
        guid = Guid.NewGuid().ToString();
        id = GameInfo.id;
        date = DateTime.Now.ToString("yyyy-MM-dd");
        skeletons_list = new List<Dictionary<HumanBodyBones, Vector3>>();
        rotations_list = new List<Dictionary<HumanBodyBones, Vector3>>();

    }

    public void setType(DetectSkeleton type)
    {
        this.type = string.Format("{0}", type);
    }

    public void setStage(string stage)
    {
        this.stage = stage;
    }

    public void setStartTime()
    {
        start_time = DateTime.Now.ToString("HH-mm-ss-ffff");
    }

    public void setEndTime()
    {
        end_time = DateTime.Now.ToString("HH-mm-ss-ffff");
    }

    public void setThreshold(float[] threshold)
    {
        this.threshold = threshold;
    }

    public void setAccuracy(float[] accuracy)
    {
        this.accuracy = accuracy;
    }

    public void addSkeletons(Dictionary<HumanBodyBones, Vector3> skeletons)
    {
        skeletons_list.Add(skeletons);
    }

    public void addRotations(Dictionary<HumanBodyBones, Vector3> rotations)
    {
        rotations_list.Add(rotations);
    }

    #region 紀錄記錄
    public static void save(RecordData record_data)
    {
        // [我的文件] 資料夾 \Somatosensory\Data
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Somatosensory\\Data");
        // [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)
        path = Path.Combine(path, string.Format("{0}\\{1}", GameInfo.id, DateTime.Now.ToString("yyyy-MM-dd")));
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path, string.Format("{0}_{1}.txt",
            SceneManager.GetActiveScene().name,
            DateTime.Now.ToString("HH-mm-ss-ffff")));

        // 檢查檔案是否存在，不存在則建立
        StreamWriter writer;
        if (!File.Exists(path))
        {
            writer = new FileInfo(path).CreateText();
        }
        else
        {
            writer = new FileInfo(path).AppendText();
        }

        // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
        writer.WriteLine(JsonConvert.SerializeObject(record_data));
        writer.Close();
        writer.Dispose();
    }

    #endregion

    #region 讀取記錄
    public static RecordData loadRecordData(string path)
    {
        StreamReader reader = new StreamReader(path);
        string load_data = reader.ReadToEnd().Trim();
        reader.Close();

        return JsonConvert.DeserializeObject<RecordData>(load_data);
    }

    public List<Dictionary<HumanBodyBones, Vector3>> getSkeletonsList()
    {
        return skeletons_list;
    }

    public List<Dictionary<HumanBodyBones, Vector3>> getRotationsList()
    {
        return rotations_list;
    }

    #endregion
}