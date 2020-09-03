using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETLab
{
    public class RecordData
    {
        #region 要紀錄的數據
        public string guid;
        public string id;
        public string date;
        public Pose pose;
        public GameStage stage;
        public string start_time;
        public string end_time;
        public float[] threshold;
        public float[] accuracy;
        public string remark = "";
        public List<Posture> posture_list;
        #endregion

        public RecordData()
        {
            guid = Guid.NewGuid().ToString();            
            date = DateTime.Now.ToString("yyyy-MM-dd");
            posture_list = new List<Posture>();
        }

        public RecordData(Player player)
        {
            guid = Guid.NewGuid().ToString();
            id = player.getId();
            date = DateTime.Now.ToString("yyyy-MM-dd");
            posture_list = new List<Posture>();
        }

        public void setId(string id)
        {
            this.id = id;
        }

        public void setPose(Pose pose)
        {
            this.pose = pose;
        }

        public void setStage(GameStage stage)
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

        public void setRemark(string remark)
        {
            this.remark = remark;
        }

        public void addPosture(Dictionary<HumanBodyBones, Vector3> skeletons, Dictionary<HumanBodyBones, Vector3> rotations)
        {
            Posture posture = new Posture()
            {
                skeletons = skeletons,
                rotations = rotations
            };

            posture_list.Add(posture);
        }

        public override string ToString()
        {
            FloatList float_list = new FloatList(threshold);
            float threshold_mean = float_list.geometricMean();
            float_list = new FloatList(accuracy);
            float accuracy_mean = float_list.geometricMean();
            return string.Format("id: {0}, date: {1}, pose: {2}, stage: {3}, start_time: {4}, end_time: {5}, " +
                "threshold: {6:F4}, accuracy: {7:F4}, remark: {8}", 
                id, date, pose, stage, start_time, end_time, threshold_mean, accuracy_mean, remark);
        }

        #region 紀錄記錄
        public string save(string file_id, string root = "", string dir = "")
        {
            if (root.Equals(""))
            {
                root = Path.Combine(GameInfo.record_root, "Somatosensory", "Data");
            }

            if (dir.Equals(""))
            {
                dir = Path.Combine(root, id, DateTime.Now.ToString("yyyy-MM-dd"));
            }

            if (!Directory.Exists(dir))
            {
                // 若目錄不存在，則產生
                Directory.CreateDirectory(dir);
            }

            // 檔名：場景名稱-(file_id產生瞬間的時間戳)
            // [我的文件] 資料夾 \Somatosensory\Data\(User Id)\(Date)\(場景名稱)-(時間戳).txt
            string path = Path.Combine(root, dir, string.Format("{0}-{1}.txt",
                                                                SceneManager.GetActiveScene().name, file_id));

            // 檢查檔案是否存在，不存在則建立
            StreamWriter writer;
            if (!File.Exists(path))
            {
                writer = new FileInfo(path).CreateText();
                Debug.Log(string.Format("[RecordData] path: {0}", path));
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

        public void reSave(string path)
        {
            // 檢查檔案是否存在
            StreamWriter writer = new FileInfo(path).CreateText();

            // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
            writer.WriteLine(JsonConvert.SerializeObject(this));
            writer.Close();
            writer.Dispose();
        }

        // 一個遊戲的所有紀錄皆寫完後，加上後綴"_done"，告訴其他程式已經可以上傳
        public static void finishWriting(string path)
        {
            Utils.renameSuffix(path, "done");
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

        public static List<RecordData> loadRecordDatas(string path)
        {
            List<RecordData> records = new List<RecordData>();
            StreamReader reader = new StreamReader(path);
            RecordData record;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                record = JsonConvert.DeserializeObject<RecordData>(line);
                records.Add(record);
            }

            reader.Close();
            return records;
        }

        public List<Posture> getPostureList()
        {
            return posture_list;
        }
        public int getPostureNumber()
        {
            return posture_list.Count;
        }

        public Posture getPosture(int index)
        {
            if (index < 0 || posture_list.Count <= index)
            {
                throw new IndexOutOfRangeException(string.Format("posture_list length is {0}.", posture_list.Count));
            }

            return posture_list[index];
        }

        public Dictionary<HumanBodyBones, Vector3> getSkeletons(int index)
        {
            if (index < 0 || posture_list.Count <= index)
            {
                throw new IndexOutOfRangeException(string.Format("posture_list length is {0}.", posture_list.Count));
            }

            return posture_list[index].skeletons;
        }

        public Vector3 getBonePosition(int index, HumanBodyBones bone)
        {
            if (index < 0 || posture_list.Count <= index)
            {
                throw new IndexOutOfRangeException(string.Format("posture_list length is {0}.", posture_list.Count));
            }

            if (!posture_list[index].contain(bone))
            {
                throw new KeyNotFoundException(string.Format("Without {0} in posture.", bone));
            }

            return posture_list[index].getBonePosition(bone);
        }

        public Dictionary<HumanBodyBones, Vector3> getRotations(int index)
        {
            if (index < 0 || posture_list.Count <= index)
            {
                throw new IndexOutOfRangeException(string.Format("posture_list length is {0}.", posture_list.Count));
            }

            return posture_list[index].rotations;
        }
        #endregion
    }
}
