using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace ETLab
{
    // 提供外部腳本事後定義偵測函式
    public delegate void DetectDelegate(Player player);

    // 定義修改 Flag 的函式架構
    public delegate Flag FlagDelegate(float flag);

    public delegate void IntegerEventDelegate(int val);
    public delegate void VoidEventDelegate();


    public class DetectManager : MonoBehaviour
    {
        protected static DetectManager dm_instance;

        [HideInInspector] public DetectDelegate detectManager = null;

        // 修改 Flag 的函式指針(指向實際的函式內容)
        [HideInInspector] public FlagDelegate modifyThresholdFlag = null;
        Flag flag = Flag.Matching;

        // 透過 PlayerManager 存取 Player
        PlayerManager pm;
        int n_player;

        // 透過 MultiPosture 存取多動作比較源
        MultiPosture mp;

        // 切換目前偵測之動作
        public Pose pose = Pose.None;

        // 讀取個動作所需比較關節
        Dictionary<Pose, List<HumanBodyBones>> comparing_parts_dict;

        // 註冊多動作比較
        Dictionary<Pose, List<Pose>> pose_dict;

        IntegerEvent onMatched = new IntegerEvent();
        List<IntegerEventDelegate> onMatchedFinished;

        public UnityEvent onAllMatched = new UnityEvent();
        List<VoidEventDelegate>  onAllMatchedFinished;

        UnityEvent onMatchEnded = new UnityEvent();
        List<VoidEventDelegate> onMatchEndedFinished;

        public UnityEvent onComparingPartsLoaded = new UnityEvent();
        bool[] all_matching_state;

        #region 紀錄骨架位置
        bool is_skeleton_recording = false;

        Transform bone;
        Vector3 vector3;
        Dictionary<HumanBodyBones, Vector3> skeletons;
        Dictionary<HumanBodyBones, Vector3> rotations;

        int bone_index, n_bone;
        string file_id;
        #endregion 

        private void Awake()
        {
            // 應該在同一個場景時，該文件共用同一個 file_id，才能將紀錄寫到同一個檔案中
            file_id = DateTime.Now.ToString("HH-mm-ss-ffff");
            Debug.Log(string.Format("[DetectManager] Scene: {0}, file_id: {1}",
                SceneManager.GetActiveScene().name, file_id));

            resetListener();

            // TODO: 實際偵測時才會用到，但應設置個監聽，當載入完成時通知主程式
            // 載入各個動作要比對的關節
            loadComparingParts();

            pose_dict = new Dictionary<Pose, List<Pose>>();
            registMultiPoses(Pose.RaiseTwoHands);
        }

        void Start()
        {
            DontDestroyOnLoad(this);
            if (dm_instance == null)
            {
                dm_instance = this;

                pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
                n_player = pm.getPlayerNumber();

                // 建構當下不會讀取數據，實際需要使用到前再讀取就好
                mp = new MultiPosture();

                // 當多比對標準載入完成
                mp.onMultiPostureLoaded.AddListener((Pose pose_type) => { 
                
                });

                all_matching_state = new bool[n_player];
                resetState();
                Debug.Log(string.Format("[DetectManager] Start | init all_matching_state, n_player: {0}", n_player));

                

                bool all_matched = false;
                onMatched.AddListener((int index) => {
                    Debug.Log(string.Format("[DetectManager] onMatched Listener player {0} matched.", index));

                    try
                    {
                        all_matching_state[index] = true;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Debug.LogError(string.Format("[DetectManager] onMatched Listener index: {0}, n_state: {1}",
                            index, all_matching_state.Length));
                    }

                    // 若 onMatchedFinished 不為 null 則執行此函式
                    foreach(IntegerEventDelegate e in onMatchedFinished)
                    {
                        e.Invoke(index);
                    }

                    all_matched = true;
                    foreach (bool state in all_matching_state)
                    {
                        all_matched &= state;
                    }

                    if (all_matched)
                    {
                        onAllMatched.Invoke();
                    }
                });

                // 全部通過
                onAllMatched.AddListener(() => {
                    Debug.Log(string.Format("[DetectManager] onAllMatched Listener"));

                    foreach (VoidEventDelegate e in onAllMatchedFinished)
                    {
                        e.Invoke();
                    }

                    // 全部通過時，同時也會觸發"結束配對"的事件
                    onMatchEnded.Invoke();
                });

                // 結束配對
                onMatchEnded.AddListener(()=> {
                    Debug.Log(string.Format("[DetectManager] onMatchEnded Listener"));

                    foreach(VoidEventDelegate e in onMatchEndedFinished)
                    {
                        e.Invoke();
                    }

                    // 移除偵測函式
                    releaseDetectDelegate();

                    // 移除 flag 函式
                    releaseFlagDelegate();

                    // 因全部通過而結束配對的情形
                    if (all_matched)
                    {

                    }

                    // 因超時而結束配對的情形
                    else
                    {
                        // TODO: 動作門檻值會隨著玩家表現而調整，在此情況下若未通過，則直接紀錄未通過這件事，而不去推測是做哪個動作但未成功
                        foreach(Player player in pm.getPlayers())
                        {

                        }
                    }
                });

                // 當比對關節載入完成
                onComparingPartsLoaded.AddListener(()=> { 
                
                });

                // 取得關節數量
                n_bone = pm.getPlayer(0).getBonesNumber();
                Debug.Log(string.Format("[DetectManager] Start | n_bone: {0}", n_bone));
            }
        }

        private void FixedUpdate()
        {
            // 設置了 detectManager 才能執行
            if (detectManager != null)
            {
                foreach (Player player in pm.getPlayers())
                {
                    // 處理移除偵測函式的瞬間，迴圈可能還在執行的問題
                    try
                    {                        
                        // 提供外部腳本 player 物件來進行動作比對
                        detectManager(player);
                    }catch(NullReferenceException nre)
                    {
                        Debug.Log(string.Format("[DetectManager] FixedUpdate | detectManager is null, {0}", nre.Message));
                    }
                }
            }

            // 紀錄關節數據
            if (is_skeleton_recording)
            {
                // 每次紀錄 1 位玩家
                foreach (var player in pm.getPlayers())
                {
                    // 該名玩家處在"紀錄數據"的狀態
                    if (player.isRecording())
                    {
                        // 每次紀錄 1 幀，所有關節位置，直到 is_skeleton_recording = false，最終會記錄許多幀
                        skeletons = new Dictionary<HumanBodyBones, Vector3>();
                        rotations = new Dictionary<HumanBodyBones, Vector3>();

                        for (bone_index = 0; bone_index < n_bone; bone_index++)
                        {
                            if ((bone = player.getBoneTransform(bone_index)) == null)
                            {
                                continue;
                            }

                            vector3 = bone.transform.position;
                            skeletons.Add(player.indexToBones(bone_index), vector3);

                            vector3 = bone.transform.rotation.eulerAngles;
                            rotations.Add(player.indexToBones(bone_index), vector3);
                        }

                        player.addPosture(skeletons, rotations);
                    }
                }
            }
        }

        void Update()
        {
            
        }

        private void OnDestroy()
        {
            // pm.getPlayers 回傳實際玩家陣列，可以避免結束執行時的數據自動儲存所發生的錯誤
            foreach (Player player in pm.getPlayers())
            {
                player.save();
            }
        }

        public void resetState()
        {
            // 玩家是否全部通過之資訊重置
            int i;
            for (i = 0; i < n_player; i++)
            {
                all_matching_state[i] = false;
            }

            // 玩家配對到的動作之資訊重置
            foreach (Player player in pm.getPlayers())
            {
                player.resetMatchedPose();
            }
        }

        public void resetState(Pose key)
        {
            // 取得標籤動作下的所有動作
            List<Pose> poses = getPoses(key);

            if(poses != null)
            {
                // 遍歷每位玩家
                foreach (Player player in pm.getPlayers())
                {
                    // 還原每項動作的暫存資訊
                    foreach (Pose pose in poses)
                    {
                        // 玩家該動作是否通過及正確率等資訊的重置
                        player.resetMovement(pose);
                    }
                }
            }

            resetState();
        }

        // 告訴程式"多動作的分類標籤(pose)"下包含哪些動作(pose_list)
        public void registMultiPoses(Pose pose, List<Pose> pose_list = null)
        {
            if (pose_dict.ContainsKey(pose))
            {
                if(pose_list == null)
                {
                    Debug.Log(string.Format("[DetectManager] registMultiPoses | update single pose: {0}", pose));
                    pose_dict[pose] = new List<Pose> { pose };
                }
                else
                {
                    Debug.Log(string.Format("[DetectManager] registMultiPoses | update multi pose: {0}", pose));
                    pose_dict[pose] = pose_list;
                }
            }
            else
            {
                if (pose_list == null)
                {
                    Debug.Log(string.Format("[DetectManager] registMultiPoses | add single pose: {0}", pose));
                    pose_dict.Add(pose, new List<Pose> { pose });
                }
                else
                {
                    Debug.Log(string.Format("[DetectManager] registMultiPoses | add multi pose: {0}", pose));
                    pose_dict.Add(pose, pose_list);
                }
            }
        }

        public List<Pose> getPoses(Pose pose)
        {
            if (pose_dict.ContainsKey(pose))
            {
                return pose_dict[pose];
            }
            else
            {
                Debug.LogError(string.Format("[DetectManager] getPoses | You need regist {0} by registMultiPoses.", pose));
                return null;
            }
        }

        // 遊戲場景才實作偵測函式(也可統一寫到其他地方)
        public void setDetectDelegate(DetectDelegate detect_delegate)
        {
            Debug.Log(string.Format("[DetectManager] setDetectDelegate"));
            detectManager = detect_delegate;
        }

        public void releaseDetectDelegate()
        {
            Debug.Log(string.Format("[DetectManager] releaseDetectDelegate"));
            detectManager = null;
        }

        public void setFlagDelegate(FlagDelegate flag_delegate)
        {
            Debug.Log(string.Format("[DetectManager] setFlagDelegate"));
            modifyThresholdFlag = flag_delegate;
        }

        public void updateFlag(float f)
        {
            flag = modifyThresholdFlag(f);
        }

        public void setFlag(Flag flag)
        {
            this.flag = flag;
        }

        public void releaseFlagDelegate()
        {
            Debug.Log(string.Format("[DetectManager] releaseFlagDelegate"));
            modifyThresholdFlag = null;
        }

        // TODO: loadComparingParts 改為同步讀取
        // 讀取個動作所需比較關節
        public void loadComparingParts()
        {
            Debug.Log(string.Format("[DetectManager] loadComparingParts"));
            comparing_parts_dict = new Dictionary<Pose, List<HumanBodyBones>>();

            // MovementDatas 數據儲存路徑(不會因人而異的部分)
            string path = Path.Combine(Application.streamingAssetsPath, "MovementData.txt");
            StreamReader reader = new StreamReader(path);
            string load_data = reader.ReadToEnd().Trim();
            reader.Close();
            MovementDatas datas = JsonConvert.DeserializeObject<MovementDatas>(load_data);
            MovementData data;

            foreach (Pose pose in Utils.poses)
            {
                // 數據包含該 pose 才作為
                if (datas.contain(pose))
                {
                    data = datas.get(pose);

                    //Debug.Log(string.Format("[DetectManager] load {0} into comparing_parts_dict.", pose));

                    // 載入各動作的比對關節資訊
                    comparing_parts_dict.Add(pose, data.getComparingParts());
                }
                else
                {
                    Debug.LogError(string.Format("[DetectManager] loadComparingParts | 數據中不包含動作 {0}", pose));
                }
            }

            onComparingPartsLoaded.Invoke();
        }

        // 事前該動作或標籤動作要有註冊
        public void loadMultiPosture(Pose key)
        {
            Debug.Log(string.Format("[DetectManager] loadMultiPosture(key: {0})", key));

            if (!pose_dict.ContainsKey(key))
            {
                Debug.LogError(string.Format("[DetectManager] loadMultiPosture | No {0} in pose_dict, you need to regist first.", key));
                return;
            }

            // 取得標籤動作下的多動作
            List<Pose> poses = pose_dict[key];

            foreach(Pose pose in poses)
            {
                Debug.Log(string.Format("[DetectManager] loadMultiPosture | loading {0}", pose));

                // 載入多動作比對標準
                // TODO: 實際偵測時才會用到，但應設置個監聽，當載入完成時通知主程式
                mp.loadMultiPosture(pose);

                // 玩家各自初始化
                foreach (Player player in pm.getPlayers())
                {
                    // 初始化各個 pose 的 Movement
                    player.setMovement(pose);
                }
            }

            #region Check loading 
            //foreach (Pose pose in poses)
            //{
            //    Debug.Log(string.Format("[DetectManager] loadMultiPosture | checking {0}", pose));

            //    var postures = mp.getMultiPostures(pose);
            //    Debug.Log(string.Format("[DetectManager] loadMultiPosture | #postures: {0}", postures.Count));

            //    foreach (Player player in pm.getPlayers())
            //    {
            //        var m = player.getMovement(pose);
            //        if (m != null)
            //        {
            //            Debug.Log(string.Format("[DetectManager] loadMultiPosture | Load {0} for {1} success.",
            //            pose, player.getId()));
            //        }
            //    }
            //} 
            #endregion

            Debug.Log(string.Format("[DetectManager] loadMultiPosture | loading finished."));
        }

        #region 動作匹配核心演算法        
        /// <summary>
        /// 比對動作 + 額外條件
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="target_pose">實際比對的動作(非多動作分類標籤)</param>
        /// <param name="additional_accuracy">額外條件的正確率</param>
        public void compareMovement(Player player, Pose target_pose, float additional_accuracy = 1f)
        {
            /* 比對動作的流程當中需要以下幾項：
             * Pose pose: 比對動作名稱
             * List<List<Posture>> multi_postures: 比較的標準(以前為模型，現在為真人實錄)
             * List<HumanBodyBones> comparing_parts: 比對關節
             * float[] thresholds: 門檻值
             * float[] accuracys: 正確率
             * bool[] is_matched: 姿勢匹配是否通過
             * 
             * 最新版將以上數據根據是否會因人而異、避免重複讀取等，將來源分散到各處
             * DetectManager >>
             * Pose pose: 比對動作名稱
             * List<HumanBodyBones> comparing_parts: 比對關節
             * MultiPosture >>
             * List<List<Posture>> multi_postures: 比較的標準(更進一步將數據轉為幀導向，方便依幀順序讀取)
             * 
             * Player >>
             * PlayerData >>
             * float[] thresholds: 門檻值(讀取檔案，將數值給 Movement)
             * 
             * Movement >>
             * float[] thresholds: 門檻值(提供 DetectManager 讀取)
             * float[] accuracys: 正確率
             * bool[] is_matched: 姿勢匹配是否通過
             */

            // 避免前面的玩家通過後，pose 被修改為 Pose.None 而導致沒有比對之標的
            if (target_pose == Pose.None)
            {
                return;
            }

            // 若已經通過則直接 return (等待其他玩家通過的情形)
            if (player.getMatchedPose() != Pose.None)
            {
                return;
            }

            // player 透過 target_pose 取得 movement，透過 movement 協助
            Movement movement = player.getMovement(target_pose);

            // 若 player.movement_map 不包含 target_pose，則 movement == null
            if (movement == null)
            {
                Debug.Log(string.Format("[DetectManager] compareMovement | No {0} in movement_map", target_pose));
                return;
            }

            // 從 comparing_parts_dict 讀取比較關節，避免重複讀取
            List<HumanBodyBones> comparing_parts;

            if (comparing_parts_dict.ContainsKey(target_pose))
            {
                // 讀取比較關節
                comparing_parts = comparing_parts_dict[target_pose];
            }
            else
            {
                Debug.LogError(string.Format("[DetectManager] getAccuracy | No {0} in comparing_parts_dict.", target_pose));
                return;
            }

            List<List<Posture>> multi_postures = mp.getMultiPostures(target_pose);

            if(multi_postures == null)
            {
                Debug.LogError(string.Format("[DetectManager] getAccuracy | Can't load multi_postures of {0}.", target_pose));
                return;
            }
           
            int model_idx, n_model = mp.getMultiNumber(target_pose), posture_idx;
            //Debug.Log(string.Format("[DetectManager] getAccuracy | Pose {0} 有 {1} 個比對標準.", target_pose, n_model));

            List<Posture> postures;
            FloatList acc_list;
            float acc, thres;

            // ========== 開始比對動作 ==========
            // 遍歷 ConfigData.n_posture 個分解動作
            for (posture_idx = 0; posture_idx < ConfigData.n_posture; posture_idx++)
            {
                // 多來源的第 posture_idx 個 Posture
                postures = multi_postures[posture_idx];
                acc_list = new FloatList();

                // 多標準共同衡量正確率
                for (model_idx = 0; model_idx < n_model; model_idx++)
                {                    
                    acc_list.add(getAccuracy(player, postures[model_idx], comparing_parts));
                }

                // 計算與多標準比對後的正確率
                acc = acc_list.geometricMean();

                //if (player.getId() == "9527")
                //{
                //    Debug.Log(string.Format("[DetectManager] getAccuracy | {0}, acc: {1:F8}, pose: {2}",
                //        player.getId(), acc, target_pose));
                //}

                // 記錄各個分解動作的最高值
                // 目前可返回最高值，是否利用這個最高值來對門檻值進行比較呢？>> 應該不行，不然通過的時間點會很奇怪
                movement.setHighestAccuracy(posture_idx, acc);

                // 取得當前動作門檻值，將用於比較是否當前正確率超過門檻
                thres = movement.getThreshold(posture_idx);

                #region 正確率 與 門檻值 之 比較 與 處理
                if(flag != Flag.None)
                {
                    // Matching / Modify 都會執行配對的判斷
                    // 正確率 大於等於 門檻值
                    if (acc >= thres)
                    {
                        // 紀錄"通過資訊"，以利後面判斷動作是否通過
                        movement.setMatched(posture_idx, true);
                        //Debug.Log(string.Format("[DetectManager] Matched: palyer: {0}, acc: {1:F8}", player.getId(), acc));
                    }

                    // 正確率 小於 門檻值
                    else
                    {
                        switch (flag)
                        {
                            case Flag.Modify:
                                // 尚未通過的分解動作，才會更新門檻值
                                if (!movement.isMatched(posture_idx))
                                {
                                    // 動態調整門檻值 movement.setThreshold(posture_idx)
                                    movement.setThreshold(posture_idx, acc);

                                    //if (player.getId().Equals("9527"))
                                    //{
                                    //    Debug.Log(string.Format("[DetectManager] compareMovement | Dynamic thresholds: {0}",
                                    //    Utils.arrayToString(movement.getThresholds())));
                                    //}
                                }
                                break;
                        }
                    }
                }
                #endregion
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
                player.setMatchedPose(target_pose);

                // 以前一次通過時的正確率，作為下一次的門檻初始值
                movement.setThresholds(movement.getAccuracy());

                // 更新門檻值
                player.setThresholds(target_pose, movement.getAccuracy());

                Debug.Log(string.Format("[DetectManager] compareMovement | ID: {0}", player.getId()));
                Debug.Log(string.Format("[DetectManager] compareMovement | Final accuracy: {0}",
                    Utils.arrayToString(movement.getAccuracy())));
                Debug.Log(string.Format("[DetectManager] compareMovement | Final threshold: {0}",
                    Utils.arrayToString(movement.getThresholds())));

                // 透過陣列紀錄每個玩家是否通過，個別玩家通過時觸發事件，將此這列紀錄更新，同時檢查是否全部都完成
                onMatched.Invoke(player.index());
            }
        }

        /// <summary>
        /// 取得單一姿勢正確率
        /// </summary>
        /// <param name="player">玩家</param>
        /// <param name="posture">要比對的單一姿勢</param>
        /// <param name="comparing_parts">該姿勢要比對的關節</param>
        /// <returns></returns>
        float getAccuracy(Player player, Posture posture, List<HumanBodyBones> comparing_parts)
        {
            HumanBodyBones bone;
            Vector3 player_vector, standrad_vector, s1, s2, p1, p2;

            // len: 每個動作所要比對對的關節數量不盡相同
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

        public void resetInitPos()
        {
            foreach(Player player in pm.getPlayers())
            {
                player.resetInitPos();
            }
        }

        public void resetListener()
        {
            onMatchedFinished = new List<IntegerEventDelegate>();
            onAllMatchedFinished = new List<VoidEventDelegate>();
            onMatchEndedFinished = new List<VoidEventDelegate>();
        }

        public void addOnMatchedListener(IntegerEventDelegate event_delegate)
        {
            onMatchedFinished.Add(event_delegate);
        }

        public void addOnAllMatchedFinishedListener(VoidEventDelegate event_delegate)
        {
            onAllMatchedFinished.Add(event_delegate);
        }

        public void addOnMatchEndedFinishedListener(VoidEventDelegate event_delegate)
        {
            onMatchEndedFinished.Add(event_delegate);
        }
        #endregion

        // 開始時間相同，結束時間不盡相同
        #region 紀錄玩家關節數據
        public void startRecord()
        {
            Debug.Log(string.Format("[DetectManager] startRecord"));
            is_skeleton_recording = true;

            foreach(Player player in pm.getPlayers())
            {
                player.startRecord();
            }
        }

        // 個別玩家結束偵測
        public void stopRecord(Player player, string root = "", string dir = "")
        {
            player.stoptRecord(file_id, root, dir);
        }

        // 整體偵測結束
        public void stopRecord()
        {
            is_skeleton_recording = false;
        } 
        #endregion
    }
}
