using ETLab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ETLab
{
    public enum JaNKen
    {
        ChoKi,  // 剪刀
        Guu,    // 石頭
        Paa,    // 布
        None
    }

    public class JaNKeNPoN : MonoBehaviour
    {
        public DetectManager dm;
        public PlayerManager pm;
        public Image left_image;
        public Image right_image;
        JaNKen[] result = new JaNKen[2];

        float accumulate_time = 0f;
        bool start_display = false;


        // Start is called before the first frame update
        void Start()
        {
            //pm.init(n_player: 1);
            pm.getPlayer(0).setId("9527");
            pm.getPlayer(1).setId("你要不要吃哈密瓜");

            // 註冊標籤動作和實際動作之間的鏈結
            dm.registMultiPoses(Pose.JaNKeNPoN, new List<Pose> {
                Pose.RaiseTwoHands, 
                Pose.StrikeLeft,
                Pose.StrikeRight,
                Pose.Squat
            });
            
            resetResult();

            dm.addOnMatchedListener(onMatchedListener);

            // 全部配對成功
            dm.addOnAllMatchedFinishedListener(() =>
            {
                Debug.Log(string.Format("[JaNKeNPoN] onAllMatched Listener"));
                judgeResult();
            });

            // 可以處理因超時而結束偵測的情況
            dm.addOnMatchEndedFinishedListener(() =>
            {
                Debug.Log(string.Format("[JaNKeNPoN] onMatchEnded Listener"));

                // 還原配對過程中的暫存資訊
                dm.resetState(Pose.JaNKeNPoN);
                //resetResult();
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                _ = dm.loadMultiPosture(Pose.JaNKeNPoN);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                dm.setDetectDelegate(defaultDetect);
                start_display = true;
            }

            //if (start_display)
            //{
            //    accumulate_time += Time.deltaTime;

            //    if (accumulate_time > 1f)
            //    {
            //        //List<Pose> poses = new List<Pose> { Pose.RaiseTwoHands };
            //        List<Pose> poses = new List<Pose> { Pose.StrikeLeft, Pose.StrikeRight };
            //        //List<Pose> poses = new List<Pose> { Pose.Squat };
            //        Movement movement;
            //        Player player = pm.getPlayer(0);

            //        foreach (Pose pose in poses)
            //        {
            //            movement = player.getMovement(pose);
            //            float[] thresholds = movement.getThresholds();

            //            Debug.Log(string.Format("[JaNKeNPoN] Update | id: {0}, pose: {1}, accuracy: {2}, thresholds: {3}",
            //                player.getId(), pose, Utils.arrayToString(movement.getAccuracy()), Utils.arrayToString(thresholds)));
                        
            //            int len = thresholds.Length;
            //            for (int i = 0; i < len; i++)
            //            {
            //                thresholds[i] *= 0.99f;
            //            }
            //            movement.setThresholds(thresholds);
            //        }

            //        accumulate_time = 0f;
            //    }
            //}
        }

        // ====================================================
        Flag modifingFlag(float f)
        {
            if(accumulate_time > 3f)
            {
                accumulate_time = 0f;
                return Flag.Modify;
            }
            else
            {
                return Flag.Matching;
            }            
        }

        void defaultDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(Pose.JaNKeNPoN);

            accumulate_time += Time.deltaTime;

            foreach (Pose pose in poses)
            {
                dm.compareMovement(player, pose);
            }
        }

        void onMatchedListener(int index)
        {
            Debug.Log(string.Format("[JaNKeNPoN] onMatched Listener: player {0} matched.", index));
            Pose pose = pm.getPlayer(index).getMatchedPose();

            switch (pose)
            {
                case Pose.RaiseTwoHands:
                    result[index] = JaNKen.Paa;
                    break;
                case Pose.StrikeLeft:
                case Pose.StrikeRight:
                    result[index] = JaNKen.ChoKi;
                    break;
                case Pose.Squat:
                    result[index] = JaNKen.Guu;
                    break;
                default:
                    Debug.LogError(string.Format("[JaNKeNPoN] Start | Something wrong, index should not be {0}.", index));
                    break;
            }

            showResult(index, result[index]);
            Debug.Log(string.Format("[JaNKeNPoN] Start | Player 0, pose: {0}, JaNKen: {1}", pose, result[0]));

            ////////////////////////////////////////////////
            System.Random random = new System.Random();
            int p = random.Next(0, 4);
            List<Pose> poses = dm.getPoses(Pose.JaNKeNPoN);
            pose = poses[p];

            switch (pose)
            {
                case Pose.RaiseTwoHands:
                    result[1] = JaNKen.Paa;
                    break;
                case Pose.StrikeLeft:
                case Pose.StrikeRight:
                    result[1] = JaNKen.ChoKi;
                    break;
                case Pose.Squat:
                    result[1] = JaNKen.Guu;
                    break;
                default:
                    Debug.LogError(string.Format("[JaNKeNPoN] Start | Something wrong, index should not be {0}.", index));
                    break;
            }

            showResult(1, result[1]);
            Debug.Log(string.Format("[JaNKeNPoN] Start | Player 1, pose: {0}, JaNKen: {1}", pose, result[1]));

            start_display = false;
            dm.onAllMatched.Invoke();
        }

        void judgeResult()
        {
            JaNKen left = result[0], right = result[1];

            if (left == right)
            {
                Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 平手"));
                return;
            }

            switch (left)
            {
                case JaNKen.ChoKi:
                    switch (right)
                    {
                        case JaNKen.Guu:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 1 獲勝。"));
                            break;
                        case JaNKen.Paa:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 0 獲勝。"));
                            break;
                        default:
                            break;
                    }
                    break;
                case JaNKen.Guu:
                    switch (right)
                    {
                        case JaNKen.ChoKi:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 0 獲勝。"));
                            break;
                        case JaNKen.Paa:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 1 獲勝。"));
                            break;
                        default:
                            break;
                    }
                    break;
                case JaNKen.Paa:
                    switch (right)
                    {
                        case JaNKen.ChoKi:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 1 獲勝。"));
                            break;
                        case JaNKen.Guu:
                            Debug.Log(string.Format("[JaNKeNPoN] judgeResult | 玩家 0 獲勝。"));
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        void showResult(int index, JaNKen janken)
        {
            if (index == 0)
            {
                left_image.sprite = Resources.Load<Sprite>(janken.ToString());
            }
            else
            {
                right_image.sprite = Resources.Load<Sprite>(janken.ToString());
            }
        }

        void resetResult()
        {
            result[0] = JaNKen.None;
            result[1] = JaNKen.None;
            left_image.sprite = null;
            right_image.sprite = null;
        }  
    }

}