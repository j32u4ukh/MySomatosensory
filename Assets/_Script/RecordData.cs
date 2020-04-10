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
    public string save(string file_id)
    {
        // 資料儲存目錄
        // [我的文件] 資料夾 \Somatosensory\Data
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Somatosensory\\Data");
        // [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)
        directory = Path.Combine(directory, string.Format("{0}\\{1}", GameInfo.id, DateTime.Now.ToString("yyyy-MM-dd")));
        if (!Directory.Exists(directory))
        {
            // 若目錄不存在，則產生
            Directory.CreateDirectory(directory);
        }

        // 檔名：場景名稱-(file_id產生瞬間的時間戳)
        // [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)\(場景名稱)-(時間戳).txt
        string path = Path.Combine(directory, string.Format("{0}-{1}.txt", SceneManager.GetActiveScene().name, file_id));

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
        writer.WriteLine(JsonConvert.SerializeObject(this));
        writer.Close();
        writer.Dispose();

        // 回傳檔案路徑
        return path;
    }

    // 一個遊戲的所有紀錄皆寫完後，加上後綴"_done"，告訴其他程式已經可以上傳
    public static void finishWriting(string path) {
        // ex path: [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)\(場景名稱)-(時間戳).txt
        FileInfo file_info = new FileInfo(path);
        // ex file_name: (場景名稱)-(時間戳)
        string file_name = file_info.Name.Split('.')[0];
        // ex new_path: [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)\(場景名稱)-(時間戳)_done.txt
        string new_path = Path.Combine(file_info.DirectoryName, string.Format("{0}_done.txt", file_name));
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