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

        // 
        private List<List<Posture>> multi_postures;

        [Header("比對關節")]
        private List<HumanBodyBones> comparing_parts;

        [Header("是否有額外條件")]
        private bool has_additional_condition;

        // 門檻值
        private float[] thresholds;

        // 正確率
        private float[] accuracys;

        // 動作數量
        private int n_posture;

        // 姿勢匹配是否通過
        private bool[] is_matched;

        // 額外條件是否通過
        private bool is_additional_matched;

        public Movement(Pose pose, int n_posture=30)
        {
            this.pose = pose;

            // 模型數量
            this.n_posture = n_posture;

            resetState();

            multi_postures = loadMultiPosture();
        }

        public List<List<Posture>> getMultiPosture()
        {
            return multi_postures;
        }

        public int getMovementNumber()
        {
            return n_posture;
        }

        public void setThresholds(float[] _thresholds)
        {
            int _length = _thresholds.Length;
            thresholds = new float[_length];
            for(int i = 0; i < _length; i++)
            {
                thresholds[i] = _thresholds[i];
            }

            if (has_additional_condition)
            {
                // 額外條件：移動距離 是否達到要求距離
                thresholds[_length - 1] = 1f;
            }
        }

        public float[] getThresholds()
        {
            return thresholds;
        }

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

        public void setAccuracy(int index, float value)
        {
            try
            {
                accuracys[index] = value;
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

        public void setComparingParts(List<HumanBodyBones> comparing_parts)
        {
            this.comparing_parts = comparing_parts;
        }

        public List<HumanBodyBones> getComparingParts()
        {
            return comparing_parts;
        }

        public void setAddtionalCondition(bool condition)
        {
            has_additional_condition = condition;
        }

        public bool hasAddtionalCondition()
        {
            return has_additional_condition;
        }

        public bool isMatched(int index)
        {
            return is_matched[index];
        }

        public void setMatched(int index, bool status)
        {
            is_matched[index] = status;
        }

        public bool isAddtionalMatched()
        {
            return is_additional_matched;
        }

        public void resetState()
        {
            // 是否通過
            is_matched = new bool[n_posture];

            // 初始化正確率
            accuracys = new float[n_posture];

            is_additional_matched = !has_additional_condition;
        }

        #region 讀取數據
        public List<List<Posture>> loadMultiPosture()
        {
            List<List<Posture>> multi_postures = new List<List<Posture>>();

            // Posture 數據儲存路徑
            string path = Path.Combine(Application.streamingAssetsPath, "MovementData", string.Format("{0}.txt", pose));

            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                string line;
                RecordData record_data;
                List<Posture> posture_list;

                while (reader.Peek() >= 0)
                {
                    // 一行是一筆紀錄
                    line = reader.ReadLine().Trim();
                    //Debug.Log(line);
                    record_data = JsonConvert.DeserializeObject<RecordData>(line);
                    posture_list = record_data.posture_list;

                    multi_postures.Add(posture_list);
                }

                reader.Close();                
            }

            return multi_postures;
        }
        #endregion

        public void save()
        {
            MovementData data = new MovementData();
            data.set(comparing_parts, has_additional_condition);

            MovementDatas datas = new MovementDatas();
            datas.set(pose, data);
            datas.save();
        }
    }

    public class MovementData
    {
        // 儲存"不會"因人而異的數值

        #region 要紀錄的數據
        // 比對關節
        public List<HumanBodyBones> comparing_parts;

        // 是否有額外條件
        public int has;
        public bool has_additional_condition;
        #endregion

        public MovementData()
        {
            comparing_parts = new List<HumanBodyBones>();
            //has_additional_condition = false;
        }

        public MovementData(List<HumanBodyBones> comparing_parts, bool has_additional_condition)
        {
            this.comparing_parts = comparing_parts;
            this.has_additional_condition = has_additional_condition;

            if (has_additional_condition)
            {
                has = 1;
            }
            else
            {
                has = 0;
            }
        }

        public void set(List<HumanBodyBones> comparing_parts, bool has_additional_condition)
        {
            this.comparing_parts = comparing_parts;
            this.has_additional_condition = has_additional_condition;

            if (has_additional_condition)
            {
                has = 1;
            }
            else
            {
                has = 0;
            }
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

        public bool contain(Pose pose) {
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
}
