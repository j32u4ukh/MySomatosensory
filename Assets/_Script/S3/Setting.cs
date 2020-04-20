using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    /*Include all kind of setting*/
    public class Setting : MonoBehaviour
    {
        public Player player;

        private void Start()
        {
            //setPlayerData("9527");
            testPlayerData("9527");
        }

        void setPlayerData(string id)
        {
            player.setId(id);
            player.setGameStage(GameStage.Test);
            // TODO: 遊戲過程中，動態調整門檻值
            player.save();
        }

        void testPlayerData(string id)
        {
            player.setId(id);
            player.init();

            GameStage game_stage = player.getGameStage();
            print(string.Format("GameStage: {0}", game_stage));

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

            Movement movement;
            foreach (Pose pose in poses)
            {
                movement = player.getMovement(pose);
                print(string.Format("{0}: {1}", pose, Utils.arrayToString(movement.getThresholds())));
                print(string.Format("{0}: {1}", pose, movement.getThreshold(0)));
                
                //List<List<Posture>> postures = movement.loadMultiPosture();


                //if (postures.Count != 0)
                //{
                //    int l = 0;
                //    foreach (List<Posture> list in postures)
                //    {
                //        l++;
                //        print(string.Format("list {0}: {1}", l, list.Count));
                //        Dictionary<HumanBodyBones, Vector3> skeletons = list[0].skeletons;
                //        Dictionary<HumanBodyBones, Vector3> rotations = list[0].rotations;

                //        foreach(var key in skeletons.Keys)
                //        {
                //            print(string.Format("skeletons {0}: {1}", key, skeletons[key]));
                //        }

                //        foreach (var key in rotations.Keys)
                //        {
                //            print(string.Format("rotations {0}: {1}", key, rotations[key]));
                //        }
                //    }
                //}


            }

        }

        void setMovementData()
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
            data = new MovementData(comparing_parts, true);
            datas.set(pose, data);

            // 右腳單腳跳
            pose = Pose.HopRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, true);
            datas.set(pose, data);

            // 左打擊
            pose = Pose.StrikeLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 右打擊
            pose = Pose.StrikeRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 左踢
            pose = Pose.KickLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 右踢
            pose = Pose.KickRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 舉左手
            pose = Pose.RaiseLeftHand;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 舉右手
            pose = Pose.RaiseRightHand;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 左揮動(水平)
            pose = Pose.WaveLeft;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 右揮動(水平)
            pose = Pose.WaveRight;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 揮動(垂直)
            pose = Pose.VerticalWave;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 舉雙手
            pose = Pose.RaiseTwoHands;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 雙腳跳
            pose = Pose.Jump;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, true);
            datas.set(pose, data);

            // 跑
            pose = Pose.Run;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 跨跳
            pose = Pose.CrossJump;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, true);
            datas.set(pose, data);

            // 伸展
            pose = Pose.Stretch;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 蹲下
            pose = Pose.Squat;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 運球
            pose = Pose.Dribble;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.RightLowerArm,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 走路
            pose = Pose.Walk;
            comparing_parts = new List<HumanBodyBones>() {
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.RightLowerLeg,
            };
            data = new MovementData(comparing_parts, false);
            datas.set(pose, data);

            // 檔案儲存
            datas.save();
            print("MovementDatas saved.");
        }
    }
}
