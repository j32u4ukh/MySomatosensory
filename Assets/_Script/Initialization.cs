using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ETLab
{
    public class Initialization : MonoBehaviour
    {
        public PlayerManager pm;
        Player player;
        public string id = "9527";
        public Pose pose = Pose.None;


        // Start is called before the first frame update
        void Start()
        {
            initPlayerData();
            //initMovementDatas();
            //checkMovementDatas();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 初始化玩家各動作門檻值
        /// </summary>
        void initPlayerData()
        {
            pm.init(n_player: 1);
            player = pm.getPlayer(0);

            // setId 的同時會建立 PlayerData，載入或初始化 game_stage 和 thresholds
            // 初始門檻值定義在 ConfigData.init_threshold，game_stage 則預設為 GameStage.Start
            player.setId(id);

            // 修改 game_stage 為 GameStage.Test
            player.setGameStage(GameStage.Test);

            float[] thresholds;
            // Utils.poses: 為實際動作，不包含動作標籤
            foreach (Pose pose in Utils.poses)
            {
                thresholds = player.getThresholds(pose);
                Debug.Log(string.Format("[Initialization] initPlayerData | Pose: {0}, thresholds: {1}", 
                    pose.ToString(), Utils.arrayToString(thresholds)));
            }

            // 將玩家數據儲存
            player.save();
        }

        /// <summary>
        /// 初始化各個動作的比對關節
        /// </summary>
        void initMovementDatas()
        {
            MovementDatas datas = new MovementDatas();
            //set(Pose pose, MovementData data)
            Pose pose;
            MovementData data;
            List<HumanBodyBones> comparing_parts;

            // 左腳單腳跳
            pose = Pose.HopLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 右腳單腳跳
            pose = Pose.HopRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 左打擊
            pose = Pose.StrikeLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 右打擊
            pose = Pose.StrikeRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 左踢
            pose = Pose.KickLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 右踢
            pose = Pose.KickRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 舉左手
            pose = Pose.RaiseLeftHand;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 舉右手
            pose = Pose.RaiseRightHand;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 左揮動(水平)
            pose = Pose.WaveLeft;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 右揮動(水平)
            pose = Pose.WaveRight;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 揮動(垂直)
            pose = Pose.VerticalWave;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 舉雙手
            pose = Pose.RaiseTwoHands;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 雙腳跳
            pose = Pose.Jump;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 跑
            pose = Pose.Run;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 跨跳
            pose = Pose.CrossJump;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 伸展
            pose = Pose.Stretch;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 蹲下
            pose = Pose.Squat;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 運球
            pose = Pose.Dribble;
            comparing_parts = new List<HumanBodyBones>() {
                // Arm
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
                // Head
                HumanBodyBones.Head,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 走路
            pose = Pose.Walk;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 檔案儲存
            datas.save();
            print("MovementDatas updated.");
        }

        void checkMovementDatas()
        {
            pm.init(n_player: 1);
            player = pm.getPlayer(0);

            // setId 的同時會建立 PlayerData，載入或初始化 game_stage 和 thresholds
            // 初始門檻值定義在 ConfigData.init_threshold，game_stage 則預設為 GameStage.Start
            player.setId(id);

            // 修改 game_stage 為 GameStage.Test
            player.setGameStage(GameStage.Test);

            // MovementDatas 數據儲存路徑(不會因人而異的部分)
            string path = Path.Combine(Application.streamingAssetsPath, "MovementData.txt");
            StreamReader reader = new StreamReader(path);
            string load_data = reader.ReadToEnd().Trim();
            reader.Close();

            MovementDatas datas = JsonConvert.DeserializeObject<MovementDatas>(load_data);
            MovementData data;
            List<HumanBodyBones> comparing_parts;

            foreach (Pose pose in Utils.poses)
            {
                // 數據包含該 pose 才作為
                if (datas.contain(pose))
                {
                    data = datas.get(pose);
                    comparing_parts = data.getComparingParts();

                    Debug.Log(string.Format("[Initialization] checkMovementDatas | {0}", 
                        comparingPartsToString(pose, comparing_parts)));
                }
                else
                {
                    Debug.LogError(string.Format("[Initialization] checkMovementDatas | 數據中不包含動作 {0}", pose));
                }
            }
        }

        string comparingPartsToString(Pose pose, List<HumanBodyBones> comparing_parts)
        {
            StringBuilder sb = new StringBuilder();
            int i, len = comparing_parts.Count;
            sb.Append(string.Format("Pose: {0}, {1} comparing parts: [", pose, len));

            for(i = 0; i < len - 1; i++)
            {
                sb.Append(comparing_parts[i].ToString());
                sb.Append(", ");
            }

            sb.Append(comparing_parts[i].ToString());
            sb.Append("]");
            return sb.ToString();
        }
    }
}
