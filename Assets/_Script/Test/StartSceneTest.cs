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
            // TODO: PlayerManager 調整玩家人數，不須動態調整，但希望操作上彈性一點
            pm.getPlayer(0).setId("9527");
            pm.getPlayer(1).setId("你要不要吃哈密瓜");
            dm.setFlagDelegate(modifingFlag);

            dm.onMatched.AddListener((int index) => {
                Debug.Log(string.Format("[StartSceneTest] onMatched Listener: player {0} matched.", index));
            });

            // 實務上，需要還原偵測狀態的情況，除了所有人都配對成功，也有可能是因為時間超時
            // TODO: 處理因超時而結束偵測的情況
            dm.onAllMatched.AddListener(()=> {
                Debug.Log(string.Format("[StartSceneTest] onAllMatched Listener"));
                dm.releaseDetectDelegate();
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

            return Flag.None;
        }

        //////////////////////////////////////////////////
        //////////////////////////////////////////////////
        void defaultDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(pose);
            accumulate_time += Time.deltaTime;
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
