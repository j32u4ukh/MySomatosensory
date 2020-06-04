using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETLab
{
    /// <summary>
    /// Player 包含 PlayerData，Player 透過 PlayerData 來存取目前階段以及各動作的門檻值
    /// </summary>
    public class PlayerData
    {
        #region PlayerData 所包含的數據
        public string id;
        public GameStage game_stage;

        // 儲存因人而異的數值
        public Dictionary<Pose, float[]> thresholds;
        #endregion

        string path;

        public PlayerData()
        {

        }

        public PlayerData(string id)
        {
            this.id = id;
            path = Path.Combine(Application.streamingAssetsPath, "PlayerData", string.Format("{0}.txt", id));

            // 載入或初始化 game_stage 和 thresholds
            loadPlayerData();
        }

        public void loadPlayerData()
        {
            if (File.Exists(path))
            {
                Debug.Log(string.Format("[PlayerData] loadPlayerData | load player data from: {0}", path));

                // 檔案存在才將數據導入
                StreamReader reader = new StreamReader(path);
                string load_data = reader.ReadToEnd().Trim();
                reader.Close();
                PlayerData data = JsonConvert.DeserializeObject<PlayerData>(load_data);
                game_stage = data.game_stage;
                thresholds = data.thresholds;
            }
            else
            {
                Debug.Log(string.Format("[PlayerData] loadPlayerData | init player data"));

                // 檔案不存在則協助初始化
                game_stage = GameStage.Start;
                thresholds = new Dictionary<Pose, float[]>();
                float[] thres;
                int res;

                foreach (Pose pose in Utils.poses)
                {
                    thres = new float[ConfigData.n_posture];
                    for (res = 0; res < ConfigData.n_posture; res++)
                    {
                        thres[res] = ConfigData.init_threshold;
                    }
                    thresholds.Add(pose, thres);
                }
            }
        }

        public void setGameStage(GameStage game_stage)
        {
            this.game_stage = game_stage;
        }

        public void setThresholds(Pose pose, float[] thres)
        {
            if (thresholds.ContainsKey(pose))
            {
                // 若原本就有該動作的數據，則更新內容
                thresholds[pose] = thres;
            }
            else
            {
                // 若原本沒有這個動作的數據，則新增進去
                thresholds.Add(pose, thres);
            }
        }

        public void setThreshold(Pose pose, int index, float thres)
        {
            if (thresholds.ContainsKey(pose))
            {
                // 若原本就有該動作的數據，則更新內容
                thresholds[pose][index] = thres;
            }
            else
            {
                // 若原本沒有這個動作的數據，則印出錯誤訊息
                Debug.LogError(string.Format("[PlayerData] setThreshold(pose: {0}, index: {1}, thres: {2:F8})", 
                    pose, index, thres));
            }
        }

        public float[] getThresholds(Pose pose)
        {
            if (thresholds.ContainsKey(pose))
            {
                return thresholds[pose];
            }
            else
            {
                return new float[ConfigData.n_posture];
            }
        }

        public float getThreshold(Pose pose, int index)
        {
            if (thresholds.ContainsKey(pose))
            {
                return thresholds[pose][index];
            }
            else
            {
                Debug.LogError(string.Format("[PlayerData] getThreshold(pose: {0}, index: {1})", pose, index));
                return 1f;
            }
        }

        public void save()
        {
            // TODO: 或許每個關卡各自會告訴玩家目前的 GameStage?
            string scene = SceneManager.GetActiveScene().name;

            switch (scene)
            {
                case "":
                    game_stage = GameStage.Test;
                    break;
                default:
                    game_stage = GameStage.Test;
                    break;
            }

            // 無論檔案是否存在，不存在則建立，存在則覆寫
            StreamWriter writer = new FileInfo(path).CreateText();

            // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
            writer.WriteLine(JsonConvert.SerializeObject(this));
            writer.Close();
            writer.Dispose();
        }
    }
}
