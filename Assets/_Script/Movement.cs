using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETLab
{
    public struct Posture
    {
        public Dictionary<HumanBodyBones, Vector3> skeletons;
        public Dictionary<HumanBodyBones, Vector3> rotations;

        public bool contain(HumanBodyBones bone)
        {
            return skeletons.ContainsKey(bone);
        }

        public Vector3 getBonePosition(HumanBodyBones bone)
        {
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

        // 門檻值(由 PlayerData 提供數據)
        private float[] thresholds;

        // 正確率
        private float[] accuracys;

        // 姿勢匹配是否通過
        private bool[] is_matched;

        // 額外條件是否通過
        private bool is_additional_matched;

        public Movement(Pose pose)
        {
            this.pose = pose;
            resetState();
        }

        // 以陣列初始化門檻值
        public void setThresholds(float[] _thresholds)
        {
            int _length = _thresholds.Length;
            thresholds = new float[_length];
            for (int i = 0; i < _length; i++)
            {
                thresholds[i] = _thresholds[i];
            }
        }

        /// <summary>
        /// 利用能力估計函數，調整門檻值
        /// </summary>
        /// <param name="index">門檻索引值</param>
        /// <param name="acc">玩家正確率</param>
        public void setThreshold(int index, float acc)
        {
            /*
             * theta: 正確率(考生能力)
             * beta: 門檻值(考題難度)
             * P: 通過機率
             P = e^(theta - beta) / (1 + e^(theta - beta))
             P + P * e^(theta - beta) = e^(theta - beta)
             P = (1 - P) * e^(theta - beta)
             P / (1 - P) = e^(theta - beta)
             ln(P) - ln(1 - P) = theta - beta
             beta = theta - ln(P) + ln(1 - P)
             */
            try
            {
                // 取得當前門檻 alpha 值
                float beta = Utils.pToAlpha(thresholds[index]);

                float theta = Utils.pToAlpha(acc);

                // 計算 P 值(玩家在當前 beta 值的條件下，通過的機率)
                float P = Utils.getIrtValue(theta, beta);

                // 0.5f 的情況為 theta == beta，與門檻值的下限無關
                // P > 0.5f: 能力較門檻高，需要上調門檻值
                if (P > 0.5f)
                {
                    Debug.Log(string.Format("[Movement] setThreshold | index: {0}, theta: {1:F8}, beta: {2:F8}, P: {3:F8}",
                                            index, theta, beta, P));

                    P = Mathf.Max(0.5f, P - ConfigData.learning_rate);
                }

                // P < 0.5f: 能力較門檻低，需要下調門檻值
                else if (P < 0.5f)
                {
                    P = Mathf.Min(0.5f, P + ConfigData.learning_rate);
                }

                // 根據新的 P 值，更新 beta 值
                beta = theta - Mathf.Log(P) + Mathf.Log(1f - P);

                // 更新門檻值(至少大於 ConfigData.min_threshold)
                thresholds[index] = Mathf.Max(Utils.alphaToP(beta), ConfigData.min_threshold);
                //Debug.Log(string.Format("[Movement] setThreshold | update value -> " +
                //    "P : {0:F4}, beta: {1:F4}, thresholds: {2:F8}, acc: {3:F8}", P, beta, thresholds[index], acc));
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
                Debug.LogError(string.Format("[Movement] getThreshold(index: {0})", index));
                return 0f;
            }
        }

        // 紀錄正確率的最高值
        public float setHighestAccuracy(int index, float value)
        {
            try
            {
                // 新數值較大才更新
                if (value > accuracys[index])
                {
                    accuracys[index] = value;
                }
            }
            catch (IndexOutOfRangeException)
            {
                Debug.LogError(string.Format("[Mo]"));
            }

            return accuracys[index];
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

        /// <summary>
        /// 計算正確率與門檻值之間的落差，還差多少才會超過門檻值
        /// </summary>
        /// <returns>正確率與門檻值之間的平均落差</returns>
        public float getGap()
        {
            int i, len = accuracys.Length;
            float total_gap = 0, gap;

            for(i = 0; i < len; i++)
            {
                gap = thresholds[i] - accuracys[i];

                if(gap > 0)
                {
                    total_gap += gap;
                }
            }

            return total_gap / len;
        }

        public Pose getPose()
        {
            return pose;
        }

        // 檢查是否所有動作皆通過
        public bool isMatched()
        {
            bool pass = true;

            int i;
            for (i = 0; i < ConfigData.n_posture; i++)
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

        public void resetState()
        {
            // 是否通過
            is_matched = new bool[ConfigData.n_posture];

            // 初始化正確率
            accuracys = new float[ConfigData.n_posture];
        }
    }

    public class MultiPosture
    {
        Dictionary<Pose, List<List<Posture>>> index_oriented_dict;
        public PoseEvent onMultiPostureLoaded = new PoseEvent();

        public MultiPosture()
        {
            index_oriented_dict = new Dictionary<Pose, List<List<Posture>>>();
        }

        #region 讀取數據
        // TODO: loadMultiPosture 改為同步讀取
        // 需要真人預錄才會有數據
        public void loadMultiPosture(Pose pose)
        {
            Debug.Log(string.Format("[MultiPosture] loadMultiPosture(pose: {0})", pose));
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
                        posture_list = Utils.sampleList(posture_list, ConfigData.n_posture);

                        multi_postures.Add(posture_list);
                    }
                }
            }
            else
            {
                Debug.Log(string.Format("[Movement] loadMultiPosture | MovementData {0} is not exist.", pose.ToString()));
            }

            // 依照分解動作的順序，存取比對標準
            Debug.Log(string.Format("[MultiPosture] loadMultiPosture | transform to index oriented"));
            index_oriented_dict.Add(pose, transformIndexOriented(multi_postures));

            onMultiPostureLoaded.Invoke(pose);
        }

        // 需要真人預錄才會有數據
        public List<List<Posture>> transformIndexOriented(List<List<Posture>> multi_postures)
        {
            Debug.Log(string.Format("[MultiPosture] transformIndexOriented | n_model: {0}", multi_postures.Count));
            List<List<Posture>> index_oriented_postures = new List<List<Posture>>();
            List<Posture> model_oriented;
            List<Posture> index_oriented;
            int index, m, n_model = multi_postures.Count;

            // 遍歷 PlayerData.n_action 個分解動作
            for (index = 0; index < ConfigData.n_posture; index++)
            {
                index_oriented = new List<Posture>();

                // 遍歷 n_model 個比對標準
                for (m = 0; m < n_model; m++)
                {
                    model_oriented = multi_postures[m];
                    index_oriented.Add(model_oriented[index]);
                }

                // 依照分解動作的順序，將多個標準依序放入 index_oriented_postures
                index_oriented_postures.Add(index_oriented);
            }

            return index_oriented_postures;
        }

        public List<List<Posture>> getMultiPostures(Pose pose)
        {
            if (index_oriented_dict.ContainsKey(pose))
            {
                return index_oriented_dict[pose];
            }

            Debug.LogError(string.Format("[MultiPosture] getMultiPostures | No {0} in index_oriented_dict.", pose));
            return null;
        }

        public List<Posture> getIndexOrientedPostures(Pose pose, int index)
        {
            return index_oriented_dict[pose][index];
        }

        // 取得多比較源的個數
        public int getMultiNumber(Pose pose)
        {
            List <Posture> posture_list = index_oriented_dict[pose][0];

            return posture_list.Count;
        }
        #endregion
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

        public List<HumanBodyBones> getComparingParts()
        {
            return comparing_parts;
        }
    }

    public class MovementDatas
    {
        // TODO: x, y, z 方向距離門檻值等其他不會因人而異的數據

        // 比對關節相關資訊
        public Dictionary<Pose, MovementData> datas;

        // 數據儲存路徑
        public static readonly string path = Path.Combine(Application.streamingAssetsPath, "MovementData.txt");

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
            // 無論是否存在，直接覆寫
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
