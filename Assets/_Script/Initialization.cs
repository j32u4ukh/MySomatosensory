using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    public class Initialization : MonoBehaviour
    {
        public PlayerManager pm;
        Player player;
        public string id;


        // Start is called before the first frame update
        void Start()
        {
            pm.init(n_player: 1);
            player = pm.getPlayer(0);

            initPlayerData(id);
            //initMovementDatas();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void initPlayerData(string id)
        {
            player.setId(id);
            player.setGameStage(GameStage.Test);

            float[] thresholds;
            foreach(Pose pose in Utils.poses)
            {
                thresholds = player.getThresholds(pose);
                Debug.Log(string.Format("[Initialization] initPlayerData | Pose: {0}, thresholds: {1}", 
                    pose.ToString(), Utils.arrayToString(thresholds)));
            }
            player.save();
        }

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
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 舉右手
            pose = Pose.RaiseRightHand;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 左揮動(水平)
            pose = Pose.WaveLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 右揮動(水平)
            pose = Pose.WaveRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 揮動(垂直)
            pose = Pose.VerticalWave;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts);
            datas.set(pose, data);

            // 舉雙手
            pose = Pose.RaiseTwoHands;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
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
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
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
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
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
            print("MovementDatas saved.");
        }
    }
}
