using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace S3
{
    public class PlayerData
    {
        #region 要紀錄的數據
        public string id;
        public GameStage game_stage;

        // 儲存因人而異的數值
        public Dictionary<Pose, float[]> thresholds;
        #endregion
        
        string path;

        public PlayerData(string id)
        {
            this.id = id;
            path = Path.Combine(Application.streamingAssetsPath, "PlayerData", string.Format("{0}.txt", id));
            loadPlayerData();
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

        public float[] getThresholds(Pose pose, int n_thresold = 30)
        {
            if (thresholds.ContainsKey(pose))
            {
                return thresholds[pose];
            }
            else
            {
                return new float[n_thresold];
            }
        }

        // TODO: 建立玩家名單，檢查玩家名稱是否註冊過，若無則呼叫此函式，以初始化 PlayerData 的檔案
        public void save()
        {
            string scene = SceneManager.GetActiveScene().name;

            switch (scene)
            {
                case "":
                    game_stage = GameStage.Game1;
                    break;
                default:
                    game_stage = GameStage.Game1;
                    break;
            }

            // 檢查檔案是否存在，不存在則建立
            StreamWriter writer = new FileInfo(path).CreateText();

            // JsonConvert.SerializeObject 將 record_data 轉換成json格式的字串
            writer.WriteLine(JsonConvert.SerializeObject(this));
            writer.Close();
            writer.Dispose();
        }

        public void loadPlayerData()
        {
            
            if (File.Exists(path))
            {
                // 檔案存在才作為
                StreamReader reader = new StreamReader(path);
                string load_data = reader.ReadToEnd().Trim();
                reader.Close();
                PlayerData data = JsonConvert.DeserializeObject<PlayerData>(load_data);
                game_stage = data.game_stage;
                thresholds = data.thresholds;
            }
            else
            {
                // 檔案不存在則協助初始化
                game_stage = GameStage.Start;
                thresholds = new Dictionary<Pose, float[]>();
            }
        }
    }
}
