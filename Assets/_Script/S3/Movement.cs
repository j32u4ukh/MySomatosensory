using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace S3
{
    public struct Posture
    {
        public Dictionary<HumanBodyBones, Vector3> skeletons;
        public Dictionary<HumanBodyBones, Vector3> rotations;

        public bool contain(HumanBodyBones bone) {
            return skeletons.ContainsKey(bone);
        }

        public Vector3 getBonePosition(HumanBodyBones bone) {
            return skeletons[bone];
        }

        public Vector3 getBoneRotation(HumanBodyBones bone)
        {
            return rotations[bone];
        }
    }

    public class Movement
    {
        [Header("動作類型")]
        private Pose pose;

        // 多比較源
        private List<List<Posture>> multi_postures;

        [Header("比對關節")]
        private List<HumanBodyBones> comparing_parts;

        // 門檻值
        private float[] thresholds;

        // 正確率
        private float[] accuracys;

        // 動作數量
        private int n_posture;

        // 姿勢匹配是否通過
        private bool[] is_matched;

        public bool has_matched;

        // 額外條件是否通過
        private bool is_additional_matched;

        public Movement()
        {
        }

        public Movement(Pose pose, int n_posture=30)
        {
            this.pose = pose;

            // 模型數量
            this.n_posture = n_posture;

            resetState();

            multi_postures = loadMultiPosture();
        }

        // 取得多比較源的全部 Posture
        public List<List<Posture>> getMultiPosture()
        {
            return multi_postures;
        }

        // 取得多比較源的個數
        public int getMultiNumber()
        {
            return multi_postures.Count;
        }

        // 根據索引值，返回多個比較源的 Posture 形成的 List<Posture>
        public List<Posture> getPostures(int index)
        {
            List<Posture> posture_list = new List<Posture>();

            foreach(List<Posture> postures in multi_postures)
            {
                posture_list.Add(postures[index]);
            }

            return posture_list;
        }

        // 取得分解動作數量
        public int getPostureNumber()
        {
            return n_posture;
        }

        // 以陣列初始化門檻值
        public void setThresholds(float[] _thresholds)
        {
            int _length = _thresholds.Length;
            thresholds = new float[_length];
            for(int i = 0; i < _length; i++)
            {
                thresholds[i] = _thresholds[i];
            }
        }

        // 設置指定的門檻值
        public void setThreshold(int index, float val)
        {
            // TODO: 利用能力估計函數，調整門檻值
            try
            {
                // 取得正確率和門檻值的平均
                val += thresholds[index];
                val /= 2;

                /* 以小於或等於兩者平均的整數做為新門檻值，
                 * 門檻高於正確率 -> 降低門檻值；正確率高於門檻 -> 提高門檻值。
                 * 若當正確率(87.5)高於門檻值(87.0)，由於平均後取較小的整數，
                 * 正確率在 87.0 ~ 87.9 之間時，門檻值都會是 87.0
                 * 限制其最小值為 0.5f
                 * 
                 * Mathf.Floor: the biggest number that "small" than val
                 */
                val = Mathf.Floor(val * 100f) / 100f;
                thresholds[index] = Mathf.Max(val, 0.6f);
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError("[setThreshold] IndexOutOfRangeException");
            }
        }

        // 取得全部門檻值
        public float[] getThresholds()
        {
            return thresholds;
        }

        // 取得指定門檻值
        public float getThreshold(int index)
        {
            try
            {
                return thresholds[index];
            }
            catch (IndexOutOfRangeException)
            {
                return 0f;
            }
        }

        // 紀錄正確率的最高值
        public void setHighestAccuracy(int index, float value)
        {
            try
            {
                // 新數值較大才更新
                if(value > accuracys[index])
                {
                    accuracys[index] = value;
                }
            }
            catch (IndexOutOfRangeException)
            {

            }
        }

        public float[] getAccuracy()
        {
            return accuracys;
        }

        public float getAccuracy(int index)
        {
            try
            {
                return accuracys[index];
            }
            catch (IndexOutOfRangeException)
            {
                return 0f;
            }
        }

        public Pose getPose()
        {
            return pose;
        }

        // TODO: 其實不會因人而異的部分直接由 DetectManager 讀取應該就好了
        public void setComparingParts(List<HumanBodyBones> comparing_parts)
        {
            this.comparing_parts = comparing_parts;
        }

        public List<HumanBodyBones> getComparingParts()
        {
            return comparing_parts;
        }

        // 檢查是否所有動作皆通過
        public bool isMatched()
        {
            bool pass = true;

            int len = getPostureNumber(), i;
            for (i = 0; i < len; i++)
            {
                pass &= is_matched[i];
            }

            // 姿勢匹配(bool[] is_matched) 與 額外條件(bool is_additional_matched) 都要通過才是真的通過
            pass &= is_additional_matched;

            return pass;
        }

        public void setMatched(int index, bool status)
        {
            is_matched[index] = status;
        }

        public bool isMatched(int index)
        {
            return is_matched[index];
        }

        public void setAddtionalMatched(bool is_matched)
        {
            is_additional_matched = is_matched;
        }

        public bool isAddtionalMatched()
        {
            return is_additional_matched;
        }

        public void resetState()
        {
            // 是否通過
            is_matched = new bool[n_posture];

            has_matched = false;

            // 初始化正確率
            accuracys = new float[n_posture];
        }

        #region 讀取數據
        // 需要真人預錄才會有數據
        public List<List<Posture>> loadMultiPosture()
        {
            List<List<Posture>> multi_postures = new List<List<Posture>>();

            // Posture 數據儲存路徑
            string dir = Path.Combine(Application.streamingAssetsPath, "MovementData", pose.ToString());

            if (Directory.Exists(dir))
            {
                string[] files = Directory.GetFiles(dir);
                StreamReader reader;
                string load_data;
                RecordData record_data;
                List<Posture> posture_list;
                
                foreach (string file in files)
                {
                    // 不包含 .meta 的才是真正的檔案
                    if (!file.Contains(".meta"))
                    {
                        reader = new StreamReader(file);
                        load_data = reader.ReadToEnd().Trim();
                        reader.Close();

                        record_data = JsonConvert.DeserializeObject<RecordData>(load_data);
                        posture_list = record_data.posture_list;

                        // 確保 posture_list 長度為 n_posture，因而每次抽樣含有一定的隨機性
                        posture_list = Utils.sampleList(posture_list, n_posture);

                        multi_postures.Add(posture_list);
                    }
                }
            }
            else
            {
                Debug.Log(string.Format("[Movement] loadMultiPosture | MovementData {0} is not exist.", pose.ToString()));
            }

            return multi_postures;
        }
        #endregion

        public void save()
        {
            MovementData data = new MovementData();
            data.set(comparing_parts);

            MovementDatas datas = new MovementDatas();
            datas.set(pose, data);
            datas.save();
        }
    }


    #region 主要用於寫出與讀取動作們的比對關節
    public class MovementData
    {
        // 儲存"不會"因人而異的數值

        #region 要紀錄的數據
        // 比對關節
        public List<HumanBodyBones> comparing_parts;
        #endregion

        public MovementData()
        {
            comparing_parts = new List<HumanBodyBones>();
        }

        public MovementData(List<HumanBodyBones> comparing_parts)
        {
            this.comparing_parts = comparing_parts;
        }

        public void set(List<HumanBodyBones> comparing_parts)
        {
            this.comparing_parts = comparing_parts;
        }
    }

    public class MovementDatas
    {
        public Dictionary<Pose, MovementData> datas;

        // 數據儲存路徑
        public static readonly string path = Path.Combine(Application.streamingAssetsPath, "MovementData.txt");

        Pose[] poses = {
            Pose.HopLeft,            // 左腳單腳跳
            Pose.HopRight,           // 右腳單腳跳

            Pose.StrikeLeft,         // 左打擊
            Pose.StrikeRight,        // 右打擊

            Pose.KickLeft,           // 左踢
            Pose.KickRight,          // 右踢

            Pose.RaiseHand,          // 舉雙手
            Pose.RaiseLeftHand,      // 舉雙手
            Pose.RaiseRightHand,     // 舉雙手

            Pose.WaveLeft,           // 左揮動(水平)
            Pose.WaveRight,          // 右揮動(水平)

            Pose.VerticalWave,       // 揮動(垂直)
            Pose.RaiseTwoHands,      // 舉雙手
            Pose.Jump,               // 雙腳跳
            Pose.Run,                // 跑
            Pose.CrossJump,          // 跨跳
            Pose.Stretch,            // 伸展
            Pose.Squat,              // 蹲下
            Pose.Dribble,            // 運球
            Pose.Walk                // 走路
        };

        public MovementDatas()
        {
            datas = new Dictionary<Pose, MovementData>();
        }

        public void set(Pose pose, MovementData data)
        {
            if (datas.ContainsKey(pose))
            {
                datas[pose] = data;
            }
            else
            {
                datas.Add(pose, data);
            }
        }

        public Dictionary<Pose, MovementData>.KeyCollection getKeys()
        {
            return datas.Keys;
        }

        public MovementData get(Pose pose)
        {
            if (datas.ContainsKey(pose))
            {
                return datas[pose];
            }

            return null;
        }

        public bool contain(Pose pose)
        {
            return datas.ContainsKey(pose);
        }

        #region 紀錄數據
        public void save()
        {
            // 檢查檔案是否存在，不存在則建立
            StreamWriter writer = new FileInfo(path).CreateText();

            // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
            writer.WriteLine(JsonConvert.SerializeObject(this));
            writer.Close();
            writer.Dispose();
        }
        #endregion

        #region 讀取記錄
        public static MovementDatas loadMovementDatas()
        {
            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                string load_data = reader.ReadToEnd().Trim();
                reader.Close();

                Debug.Log(load_data);

                return JsonConvert.DeserializeObject<MovementDatas>(load_data);
            }
            else
            {
                Debug.Log("Not exists");
                return new MovementDatas();
            }
        }

        public static Dictionary<Pose, MovementData> loadMovementData()
        {
            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                string load_data = reader.ReadToEnd().Trim();
                reader.Close();

                //Debug.Log(string.Format("Exists: {0}", path));
                Debug.Log(load_data);
                return new Dictionary<Pose, MovementData>();

                //return JsonConvert.DeserializeObject<MovementDatas>(load_data).datas;
            }
            else
            {
                Debug.Log("Not exists");
                return new Dictionary<Pose, MovementData>();
            }
        }
        #endregion
    } 
    #endregion
}
