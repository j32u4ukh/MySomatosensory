using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    public class IrtDemo : MonoBehaviour
    {
        public DetectManager dm;
        public PlayerManager pm;
        public Pose pose = Pose.RaiseTwoHands;

        string gui = "", msg1 = "", msg2 = "";
        bool matched1 = false, matched2 = false;
        float time = 0f, round_time = 0f, acc1 = 0f, thres1 = 0f, acc2 = 0f, thres2 = 0f;
        float boundary_time = 5f;

        Movement movement;
        FloatList float_list;

        // Start is called before the first frame update
        void Start()
        {
            pm.getPlayer(0).setId("9527");
            pm.getPlayer(1).setId("你要不要吃哈密瓜");
            float[] temp = new float[30];
            for(int i = 0; i < 30; i++)
            {
                temp[i] = 1f;
            }

            foreach(Player player in pm.getPlayers())
            {
                foreach(Pose p in Utils.poses)
                {
                    player.setThresholds(p, temp);
                }
            }

            // modify flag
            dm.setFlagDelegate(modifingFlag);

            // 註冊標籤動作和實際動作之間的鏈結
            //dm.registMultiPoses(Pose.IrtDemo, new List<Pose> {
            //    Pose.RaiseTwoHands,
            //    Pose.StrikeLeft,
            //    Pose.StrikeRight,
            //    Pose.Squat
            //});

            dm.registMultiPoses(Pose.RaiseTwoHands);

            dm.addOnMatchedListener(onMatchedListener);

            // 全部配對成功
            dm.addOnAllMatchedFinishedListener(() =>
            {
                Debug.Log(string.Format("[IrtDemo] onAllMatched Listener"));

            });

            // 可以處理因超時而結束偵測的情況
            dm.addOnMatchEndedFinishedListener(() =>
            {
                Debug.Log(string.Format("[IrtDemo] onMatchEnded Listener"));
                Player player = pm.getPlayer(0);
                Movement movement = player.getMovement(pose);
                float[] ac1 = movement.getAccuracy();
                //FloatList fl1 = new FloatList(ac1);
                //acc1 = fl1.mean();
                Debug.Log(string.Format("[IrtDemo] Accuracy1: {0}", Utils.arrayToString(ac1)));
                Debug.Log(string.Format("[IrtDemo] Accuracy1: {0}", Utils.arrayToString(movement.getThresholds())));

                player = pm.getPlayer(1);
                movement = player.getMovement(pose);
                Debug.Log(string.Format("[IrtDemo] Accuracy2: {0}", Utils.arrayToString(movement.getAccuracy())));
                Debug.Log(string.Format("[IrtDemo] Accuracy2: {0}", Utils.arrayToString(movement.getThresholds())));

                // 還原配對過程中的暫存資訊
                dm.resetState(Pose.RaiseTwoHands);

            });

            StartCoroutine(gameStart());
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnGUI()
        {
            if(time < boundary_time)
            {
                GUI.color = Color.black;
            }
            else
            {
                GUI.color = Color.red;
            }

            GUI.skin.label.fontSize = 60;
            msg1 = string.Format("Acc1: {0:F4}, Thres1: {1:F4}", acc1, thres1);
            msg2 = string.Format("Acc2: {0:F4}, Thres2: {1:F4}", acc2, thres2);

            if (matched1 || matched2)
            {
                if (matched1 && matched2)
                {
                    msg2 = string.Format("{0}\nAll player matched.", msg2);
                }
                else if (matched1)
                {
                    msg1 = string.Format("{0}, Player1 matched.", msg1);

                }else if (matched2)
                {
                    msg2 = string.Format("{0}, Player2 matched.", msg2);

                }
            }

            gui = string.Format("Time: {0:F2}\nPose: {1}\n{2}\n{3}", time, pose, msg1, msg2);
            GUILayout.Label(
                // text on gui
                gui,
                // start to define gui layout
                GUILayout.Width(Screen.width / 2f),
                GUILayout.Height(Screen.height / 2f));
        }

        // ====================================================
        Flag modifingFlag(float f)
        {
            if (time > boundary_time && round_time > 5f)
            {
                //round_time = 0f;

                // 配對(O), 修改門檻值(O)
                return Flag.Modify;
            }
            else
            {
                // 配對(O), 修改門檻值(X)
                return Flag.Matching;
            }
        }

        void defaultDetect(Player player)
        {
            int idx = player.index();
            List<Pose> poses = dm.getPoses(Pose.RaiseTwoHands);

            time += Time.deltaTime;
            round_time += Time.deltaTime * 10f;

            // 透過 updateFlag 將 accumulate_time 傳給 modifingFlag，根據時間調整偵測模式
            dm.updateFlag(time);

            foreach (Pose pose in poses)
            {
                // 比對動作
                dm.compareMovement(player, pose);

                // 取出玩家正確率與門檻值
                movement = player.getMovement(pose);
                float_list = new FloatList(movement.getAccuracy());

                if(idx == 0 && !matched1)
                {
                    acc1 = float_list.mean();
                }
                else if(idx == 1 && !matched2)
                {
                    acc2 = float_list.mean();
                }

                float_list = new FloatList(movement.getThresholds());

                if (idx == 0 && !matched1)
                {
                    thres1 = float_list.mean();
                }
                else if (idx == 1 && !matched2)
                {
                    thres2 = float_list.mean();
                }
            }
        }

        void onMatchedListener(int index)
        {
            //Debug.Log(string.Format("[IrtDemo] onMatched Listener: player {0} matched.", index));
            Pose pose = pm.getPlayer(index).getMatchedPose();
            Debug.Log(string.Format("[IrtDemo] onMatched Listener: player {0} matched pose: {1}.", index, pose));

            if(index == 0)
            {
                matched1 = true;

            }else if(index == 1)
            {
                matched2 = true;
            }
        }

        IEnumerator gameStart()
        {
            yield return new WaitForSecondsRealtime(Time.deltaTime);
            dm.loadMultiPosture(Pose.RaiseTwoHands);
            yield return new WaitForSecondsRealtime(Time.deltaTime);
            dm.setDetectDelegate(defaultDetect);
        }
    }
}