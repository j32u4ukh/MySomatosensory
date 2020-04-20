using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace S3
{    
    public class Player : MonoBehaviour
    {
        #region Information of player
        private string id;
        private int player_index;
        private GameStage game_stage;
        private Dictionary<Pose, Movement> movement_map;
        //private int bones_number;

        #region Data for remark
        string remark;
        #endregion
        #endregion

        #region Component of player
        protected AvatarController avatar_controller;
        protected PoseModelHelper model_helper;
        #endregion

        // 動作名稱 與 動作物件
        Pose[] poses = {
            Pose.HopLeft,            // 左腳單腳跳
            Pose.HopRight,           // 右腳單腳跳

            Pose.StrikeLeft,         // 左打擊
            Pose.StrikeRight,        // 右打擊

            Pose.KickLeft,           // 左踢
            Pose.KickRight,          // 右踢

            Pose.RaiseLeftHand,      // 舉左手
            Pose.RaiseRightHand,     // 舉右手

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

        private void Awake()
        {
            /* https://blog.csdn.net/u011185231/article/details/49523293
             * 問題: 其他腳本調用 Player 物件時產生 NullReferenceException
             * 解決方法: 把最先實例化的全部放在Awake()方法中去
             */
            avatar_controller = GetComponent<AvatarController>();
            model_helper = GetComponent<PoseModelHelper>();
            movement_map = new Dictionary<Pose, Movement>();
            
        }

        // Start is called before the first frame update
        void Start()
        {
            player_index = avatar_controller.playerIndex;

        }

        public void setId(string id)
        {
            this.id = id;
        }

        public string getId()
        {
            return id;
        }

        public int index()
        {
            return player_index;
        }

        public void setGameStage(GameStage game_stage)
        {
            this.game_stage = game_stage;
        }

        public GameStage getGameStage()
        {
            return game_stage;
        }

        #region Movement
        public Movement getMovement(Pose pose)
        {
            if (movement_map.ContainsKey(pose))
            {
                return movement_map[pose];
            }
            else
            {
                return null;
            }
        }
        #endregion


        #region PoseModelHelper
        public Transform getBoneTransform(int index)
        {
            return model_helper.GetBoneTransform(index);
        }

        public Transform getBoneTransform(HumanBodyBones bone)
        {
            int index = PoseModelHelper.boneToIndex(bone);

            if (index == -1)
            {
                return null;
            }

            return model_helper.GetBoneTransform(index);
        }

        public int getBonesNumber()
        {
            return model_helper.GetBoneTransformCount();
        }

        public HumanBodyBones indexToBones(int index)
        {
            return PoseModelHelper.indexToBone(index);
        }

        public int boneToIndex(HumanBodyBones bone)
        {
            return PoseModelHelper.boneToIndex(bone);
        }
        #endregion

        public void init()
        {
            PlayerData player_data = new PlayerData(id);
            game_stage = player_data.game_stage;

            // MovementDatas 數據儲存路徑
            string path = Path.Combine(Application.streamingAssetsPath, "MovementData.txt");
            StreamReader reader = new StreamReader(path);
            string load_data = reader.ReadToEnd().Trim();
            reader.Close();
            MovementDatas datas = JsonConvert.DeserializeObject<MovementDatas>(load_data);
            MovementData data;
            Movement movement;

            foreach (Pose pose in poses)
            {
                movement = new Movement(pose, n_posture: 30);

                // 數據包含該 pose 才作為
                if (datas.contain(pose))
                {
                    data = datas.get(pose);
                    movement.setComparingParts(data.comparing_parts);
                    movement.setAddtionalCondition(data.has_additional_condition);
                    movement.setThresholds(player_data.getThresholds(pose));
                    movement_map.Add(pose, movement);
                }
                else
                {
                    Debug.Log(string.Format("數據中不包含動作 {0}", pose));
                }
            }
        }

        public List<HumanBodyBones> getComparingParts(Pose pose)
        {
            Movement movement = getMovement(pose);

            if(movement != null)
            {
                return movement.getComparingParts();
            }

            return null;
        }

        public void save()
        {
            PlayerData player_data = new PlayerData(id);
            Movement movement;
            float[] thres;

            foreach (Pose pose in poses)
            {
                // 數據包含該 pose 才作為
                if (movement_map.ContainsKey(pose))
                {
                    movement = movement_map[pose];
                    thres = movement.getThresholds();
                    player_data.setThresholds(pose, thres);
                }
            }

            player_data.save();
        }
    }
}
