using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETLab
{
    public class StartSceneTest : MonoBehaviour
    {
        public PlayerManager pm;
        public DetectManager dm;
        float accumulate_time = 0f;
        public Pose pose;

        // Start is called before the first frame update
        void Start()
        {
            pm.getPlayer(0).setId("9527");
            pm.getPlayer(1).setId("你要不要吃哈密瓜");
            dm.setFlagDelegate(modifingFlag);
            dm.registMultiPoses(Pose.VerticalWave);
            dm.registMultiPoses(Pose.Squat);
            dm.registMultiPoses(Pose.Walk);
            dm.registMultiPoses(Pose.Run);
            dm.registMultiPoses(Pose.Hop, new List<Pose> { Pose.HopLeft, Pose.HopRight });
            dm.registMultiPoses(Pose.Jump);
            dm.registMultiPoses(Pose.Strike, new List<Pose> { Pose.StrikeLeft, Pose.StrikeRight });
            dm.registMultiPoses(Pose.Dribble);
            dm.registMultiPoses(Pose.RaiseHand, new List<Pose> { Pose.RaiseLeftHand, Pose.RaiseRightHand });

            dm.onMatched.AddListener((int index) => {
                Debug.Log(string.Format("[StartSceneTest] onMatched Listener: player {0} matched.", index));
            });
       
            dm.onAllMatched.AddListener(()=> {
                Debug.Log(string.Format("[StartSceneTest] onAllMatched Listener"));
                //dm.releaseDetectDelegate();                
            });

            // 處理因超時而結束偵測的情況
            dm.onMatchEnded.AddListener(()=> {
                Debug.Log(string.Format("[StartSceneTest] onMatchEnded Listener"));
                accumulate_time = 0f;
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log(string.Format("[StartSceneTest] Update | loadMultiPosture: {0}", pose));
                dm.loadMultiPosture(pose);
            }

            // 跳場景
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    SceneManager.LoadScene(string.Format("_Scenes/Game{0}Scene", 1));
                }
            }

            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            if (Input.GetKeyDown(KeyCode.D))
            {
                dm.setDetectDelegate(defaultDetect);
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                dm.resetInitPos();
                dm.setDetectDelegate(yAxisDetect);
            }
        }

        // 可根據需求，對何時開始更新門檻值這件事做設定
        // 累計時間超過 3 秒 Flag 改為 Flag.Modify -> 開始偵測
        Flag modifingFlag(float f)
        {
            if (f > 3f)
            {
                return Flag.Modify;
            }

            if (f > 1f)
            {
                return Flag.Matching;
            }

            return Flag.None;
        }

        //////////////////////////////////////////////////
        //////////////////////////////////////////////////
        void defaultDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(pose);
            accumulate_time += Time.deltaTime;

            // 透過 updateFlag 將 accumulate_time 傳給 modifingFlag
            dm.updateFlag(accumulate_time);

            foreach (Pose pose in poses)
            {
                dm.compareMovement(player, pose);
            }
        }

        void yAxisDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(pose);
            accumulate_time += Time.deltaTime;
            dm.updateFlag(accumulate_time);
            float y_distance = player.getDistanceY();
            float y_threshold = 0.2f;

            foreach (Pose pose in poses)
            {
                dm.compareMovement(player, pose, y_distance / y_threshold);
            }
        }

        void detectRaiseTwoHands(Player player)
        {
            List<Pose> poses = dm.getPoses(Pose.RaiseTwoHands);
            accumulate_time += Time.deltaTime;
            dm.updateFlag(accumulate_time);

            foreach (Pose pose in poses)
            {
                dm.compareMovement(player, pose);
            }
        }
    }
}
