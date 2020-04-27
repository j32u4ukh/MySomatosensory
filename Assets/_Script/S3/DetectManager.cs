using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;

namespace S3
{
    public class DetectManager : MonoBehaviour
    {
        public Pose pose = Pose.None;
        public Player[] players;
        

        // For gui
        string gui;
        FloatList acc_list;
        float acc, thres;

        // Start is called before the first frame update
        void Start()
        {
            players[0].setId("9527");
            players[0].loadData();

            acc = 0f;
            thres = 0f;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                foreach (Player player in players)
                {
                    Movement movement = player.getMovement(pose);
                    int model_idx, n_model = movement.getMultiNumber();

                    if (movement == null)
                    {
                        throw new NullReferenceException("[compareMovement] movement is null.");
                    }

                    int posture_idx = 0, n_posture = movement.getPostureNumber();
                    List<Posture> postures;

                    postures = movement.getPostures(posture_idx);
                    print(string.Format("n_posture: {0}, postures.Count: {1}, n_model: {2}", n_posture, postures.Count, n_model));

                    for (posture_idx = 0; posture_idx < n_posture; posture_idx++)
                    {
                        // 多來源的第 posture_idx 幀 Posture
                        //postures = movement.getPostures(posture_idx);
                        //print(string.Format("postures.Count: {0}", postures.Count));
                        //acc_list.clear();

                        // 多標準共同衡量正確率
                        //for (model_idx = 0; model_idx < n_model; model_idx++)
                        //{
                        //    acc_list.add(getAccuracy(player, postures[model_idx]));
                        //}

                        // 計算與多標準比對後的正確率
                        //acc = acc_list.geometricMean();

                        // 記錄各個分解動作的最高值
                        //movement.setHighestAccuracy(posture_idx, acc);

                        // 取得當前動作門檻值，並比較是否當前正確率超過門檻
                        thres = movement.getThreshold(posture_idx);
                        print(string.Format("thres {0}: {1}", posture_idx, thres));
                        //if (acc >= thres)
                        //{
                        //    // TODO: 紀錄通過資訊，以利後面判斷動作是否通過
                        //    movement.setMatched(posture_idx, true);
                        //}
                        //else
                        //{
                        //    // 動態調整門檻值 movement.setThreshold(posture_idx)
                        //    movement.setThreshold(posture_idx, acc);
                        //    print(string.Format("Dynamic thresholds: {0}", Utils.arrayToString(movement.getThresholds())));
                        //}
                    }

                    // 附加額外通關條件
                    //if (additional_accuracy >= 1f)
                    //{
                    //    movement.setAddtionalMatched(true);
                    //}

                    // TODO: 考慮只有一名玩家通過，或兩位都通過
                    // 當所有動作皆通過                       
                    if (movement.isMatched())
                    {
                        // 任一動作完成偵測，停止偵測
                        pose = Pose.None;
                    }
                    else
                    {
                        print(string.Format("Accurcy: {0}", Utils.arrayToString(movement.getAccuracy())));
                        print(string.Format("Threshold: {0}", Utils.arrayToString(movement.getThresholds())));
                    }
                }
            }

            switch (pose)
            {
                case Pose.RaiseTwoHands:
                    foreach (Player player in players)
                    {
                        compareMovement(player, Pose.RaiseTwoHands);
                    }
                    break;
                default:

                    break;
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 50;
            gui = string.Format("Pose: {0}\nAcc: {1:F4}\nThres: {2:F4}",
                pose, acc, thres);
            GUILayout.Label(
                // text on gui
                gui, 
                // start to define gui layout
                GUILayout.Width(800), 
                GUILayout.Height(500));
        }

        // 取得單一姿勢正確率
        float getAccuracy(Player player, Posture posture)
        {
            List<HumanBodyBones> comparing_parts = player.getComparingParts(pose);
            HumanBodyBones bone;
            Vector3 player_vector, standrad_vector, s1, s2, p1, p2;

            int i, len = comparing_parts.Count;
            float diff, total_diff = 0f;
            Transform trans;

            // 遍歷要比對的部位
            for (i = 0; i < len; i++)
            {
                bone = comparing_parts[i];

                if ((trans = player.getBoneTransform(bone)) == null)
                {
                    continue;
                }
                else
                {
                    p1 = trans.position;
                }

                if (!posture.contain(bone))
                {
                    continue;
                }
                else
                {
                    s1 = posture.getBonePosition(bone);
                }

                if ((i + 1) >= len)
                {
                    bone = comparing_parts[0];
                }
                else
                {
                    bone = comparing_parts[i + 1];
                }

                if ((trans = player.getBoneTransform(bone)) == null)
                {
                    continue;
                }
                else
                {
                    p2 = trans.position;
                }

                if (!posture.contain(bone))
                {
                    continue;
                }
                else
                {
                    s2 = posture.getBonePosition(bone);
                }

                //取得玩家與標準模型 目前節點(jointType)的向量
                player_vector = (p2 - p1).normalized;
                standrad_vector = (s2 - s1).normalized;

                //計算玩家骨架 與 姿勢骨架角度差距
                diff = Vector3.Angle(player_vector, standrad_vector);
                if (diff > 90f)
                {
                    diff = 90f;
                }
                total_diff += diff / 90f;
            }

            total_diff /= len;

            return 1f - total_diff;
        }

        // 比對動作 + 額外條件
        void compareMovement(Player player, Pose target_pose, float additional_accuracy = 1f)
        {
            try
            {
                // player 透過 target_pose 取得 movement
                Movement movement = player.getMovement(target_pose);
                int model_idx, n_model = movement.getMultiNumber();

                if (movement == null)
                {
                    throw new NullReferenceException("[compareMovement] movement is null.");
                }

                int posture_idx, n_posture = movement.getPostureNumber();
                List<Posture> postures;

                for (posture_idx = 0; posture_idx < n_posture; posture_idx++)
                {
                    // 多來源的第 posture_idx 幀 Posture
                    postures = movement.getPostures(posture_idx);
                    FloatList acc_list = new FloatList();

                    // 多標準共同衡量正確率
                    for (model_idx = 0; model_idx < n_model; model_idx++)
                    {
                        acc_list.add(getAccuracy(player, postures[model_idx]));
                    }

                    // 計算與多標準比對後的正確率
                    acc = acc_list.geometricMean();

                    // 記錄各個分解動作的最高值
                    movement.setHighestAccuracy(posture_idx, acc);

                    // 取得當前動作門檻值，並比較是否當前正確率超過門檻
                    thres = movement.getThreshold(posture_idx);
                    if (acc >= thres)
                    {
                        // TODO: 紀錄通過資訊，以利後面判斷動作是否通過
                        movement.setMatched(posture_idx, true);
                    }
                    else
                    {
                        // 動態調整門檻值 movement.setThreshold(posture_idx)
                        movement.setThreshold(posture_idx, acc);
                        //print(string.Format("Dynamic thresholds: {0}", Utils.arrayToString(movement.getThresholds())));
                    }
                }

                // 附加額外通關條件
                if(additional_accuracy >= 1f)
                {
                    movement.setAddtionalMatched(true);
                }

                // TODO: 考慮只有一名玩家通過，或兩位都通過
                // 當所有動作皆通過                       
                if (movement.isMatched())
                {
                    // TODO: 全部玩家或一定比例以上才停止偵測
                    // 任一動作完成偵測，停止偵測
                    pose = Pose.None;
                }
            }
            catch (KeyNotFoundException)
            {
                print(string.Format("No {0} in movement_map", pose));

            }
        }
    }
}
