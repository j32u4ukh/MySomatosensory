using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;
using Debug = UnityEngine.Debug;
using UnityEngine.Events;

namespace S3
{
    public delegate void DetectDelegate();

    public class DetectManager : MonoBehaviour
    {
        public DetectDelegate detectManager;
        bool is_manager_inited = false;

        public Pose pose = Pose.None;

        // 和其他腳本共用 PlayerManger 當中的 Player，才不用重複讀取資源
        public Player[] players;

        // 
        Dictionary<Pose, List<HumanBodyBones>> comparing_parts_dict;

        public IntegerEvent onMatched;
        public UnityEvent onAllMatched;
        bool[] all_matching_state;
        Dictionary<Pose, List<Pose>> pose_dict;

        // For gui
        string gui;

        // global variable to local variable
        float acc, thres;

        
        // Start is called before the first frame update
        void Start()
        {
            // DontDestroyOnLoad(this); 令 DetectManager 在場景轉換時不會被移除

            // 外部腳本透過呼叫 setPlayer 來告訴 DetectManager 有哪些玩家
            #region 之後會移出的部分
            players[0].setId("9527");
            players[0].loadData();

            players[1].setId("你要不要吃哈密瓜");
            players[1].loadData(); 
            #endregion

            acc = 0f;
            thres = 0f;

            onMatched = new IntegerEvent();
            onAllMatched = new UnityEvent();
            all_matching_state = new bool[players.Length];
            Debug.Log(string.Format("[DetectManager] init all_matching_state, length: {0}", players.Length));

            resetState();

            onMatched.AddListener((int index)=> {
                try
                {
                    all_matching_state[index] = true;
                }
                catch (IndexOutOfRangeException)
                {
                    Debug.LogError(string.Format("[DetectManager] onMatched Listener index: {0}, n_state: {1}", 
                        index, all_matching_state.Length));
                }

                Debug.Log(string.Format("[DetectManager] onMatched Listener player {0} matched.", index));

                bool all_matched = true;
                foreach(bool state in all_matching_state)
                {
                    all_matched &= state;
                }

                if (all_matched)
                {
                    onAllMatched.Invoke();
                }
            });

            onAllMatched.AddListener(()=> {
                Debug.Log(string.Format("[DetectManager] onAllMatched Listener"));
            });

            pose_dict = new Dictionary<Pose, List<Pose>>();
            registMultiPoses(Pose.RaiseTwoHands, new List<Pose> { Pose.RaiseTwoHands });
        }

        // Update is called once per frame
        void Update()
        {
            /*由外部腳本定義要偵測哪些動作，額外條件亦可在外部腳本計算
             * delegate void detectManager()
             * 
             * 外部腳本 >>
             * void detectRaiseTwoHands(){
             *     foreach (Player player in players)
             *     {
             *          // 計算 additional_accuracy 的情形
             *          float additional_accuracy = player.distanceY() > y_distance;
             *          compareMovement(player, Pose.RaiseTwoHands, additional_accuracy);
             *          
             *          // 計算 additional_accuracy2 的情形
             *          float additional_accuracy2 = player.distanceX() > x_distance;
             *          compareMovement(player, Pose.RaiseTwoHands, additional_accuracy);
             *     }
             * }
             */
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

            // 設置了 detectManager 才能執行
            if (is_manager_inited)
            {
                detectManager();
            }
        }

        private void OnGUI()
        {
            //GUI.color = Color.red;
            //GUI.skin.label.fontSize = 50;
            //gui = string.Format("Pose: {0}\nAcc: {1:F4}\nThres: {2:F4}",
            //    pose, acc, thres);
            //GUILayout.Label(
            //    // text on gui
            //    gui, 
            //    // start to define gui layout
            //    GUILayout.Width(800), 
            //    GUILayout.Height(500));
        }

        
        public void initDetectManager()
        {
            // 讀取各個動作的比對關節
            comparing_parts_dict = new Dictionary<Pose, List<HumanBodyBones>>();
        }

        public void initDetectDelegate(DetectDelegate detect_delegate)
        {
            is_manager_inited = true;
            detectManager = detect_delegate;
        }

        public void setPlayer(Player player, int index)
        {
            players[index] = player;
        }

        // 取得單一姿勢正確率
        float getAccuracy(Player player, Posture posture)
        {
            // : 從 comparing_parts_dict 讀取比較關節，不需由 player 來提供
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
        public void compareMovement(Player player, Pose target_pose, float additional_accuracy = 1f)
        {
            // 避免前面的玩家通過後，pose 被修改為 Pose.None 而導致沒有比對之標的
            if (target_pose == Pose.None)
            {
                return;
            }

            // player 透過 target_pose 取得 movement
            Movement movement = player.getMovement(target_pose);

            // 若 player.movement_map 不包含 target_pose，則 movement == null
            if (movement == null)
            {
                Debug.Log(string.Format("[DetectManager] compareMovement | No {0} in movement_map", target_pose));
                return;
            }

            // 若已經通過則直接 return (等待其他玩家通過的情形)
            if (movement.has_matched)
            {
                //Debug.Log(string.Format("[DetectManager] compareMovement | player {0} has_matched.", player.getId()));
                return;
            }

            int model_idx, n_model = movement.getMultiNumber(),
                posture_idx, n_posture = movement.getPostureNumber();
            List<Posture> postures;
            FloatList acc_list;

            for (posture_idx = 0; posture_idx < n_posture; posture_idx++)
            {
                // 多來源的第 posture_idx 幀 Posture
                postures = movement.getPostures(posture_idx);
                acc_list = new FloatList();

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

                // 正確率 大於等於 門檻值
                if (acc >= thres)
                {
                    // 紀錄"通過資訊"，以利後面判斷動作是否通過
                    movement.setMatched(posture_idx, true);
                }

                // 正確率 小於 門檻值
                else
                {
                    // 限定於單一動作時才調整
                    // 動態調整門檻值 movement.setThreshold(posture_idx)
                    movement.setThreshold(posture_idx, acc);
                    //Debug.Log(string.Format("[DetectManager] compareMovement | Dynamic thresholds: {0}", 
                    //    Utils.arrayToString(movement.getThresholds())));
                }
            }

            // 附加額外通關條件
            if (additional_accuracy >= 1f)
            {
                movement.setAddtionalMatched(true);
            }

            // 當所有動作皆通過                       
            if (movement.isMatched())
            {
                // 紀錄已經完成的資訊，避免重複判斷
                movement.has_matched = true;

                // 透過陣列紀錄每個玩家是否通過，個別玩家通過時觸發事件，將此這列紀錄更新，同時檢查是否全部都完成
                onMatched.Invoke(player.index());

                Debug.Log(string.Format("[DetectManager] compareMovement | ID: {0}", player.getId()));
                Debug.Log(string.Format("[DetectManager] compareMovement | Final accuracy: {0}",
                    Utils.arrayToString(movement.getAccuracy())));
            }
        }

        public void resetState()
        {
            int i, len = all_matching_state.Length;

            for(i = 0; i < len; i++)
            {
                all_matching_state[i] = false;
            }
        }

        public void resetState(Pose key)
        {
            // 初始化各個動作，是否通過 與 正確率
            List<Pose> pose_list = pose_dict[key];
            Movement m;
            foreach(Player player in players)
            {
                foreach (Pose pose in pose_list)
                {
                    m = player.getMovement(pose);
                    m.resetState();
                }
            }

            resetState();
        }

        public void registMultiPoses(Pose pose, List<Pose> pose_list)
        {
            if (pose_dict.ContainsKey(pose))
            {
                pose_dict[pose] = new List<Pose>(pose_list);
            }
            else
            {
                pose_dict.Add(pose, pose_list);
            }            
        }
    }
}
