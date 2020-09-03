using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    // 每個玩家有兩個模型，一個用來呈現(external root motion)，一個用來偵測(apply root motion)
    public class Player : MonoBehaviour
    {
        #region Information of player
        private string id;
        private int player_index;
        private PlayerData player_data;
        private Vector3? init_pos;

        // 透過 Movement 協助記錄通過與否，以及當前的正確率
        private Dictionary<Pose, Movement> movement_dict;
        public RecordData record;
        bool is_recording;
        private Pose target_pose = Pose.None;
        private Pose matched_pose = Pose.None;
        #endregion

        #region Component of player
        public AvatarController avatar_controller;
        public PoseModelHelper model_helper;
        #endregion        

        private void Awake()
        {
            player_index = avatar_controller.playerIndex;
            is_recording = false;
            record = new RecordData();
        }

        // Start is called before the first frame update
        void Start()
        {
                     
        }

        #region Player
        public void setId(string id)
        {
            Debug.Log(string.Format("[Player] setId(id: {0})", id));
            this.id = id;
            record.setId(id);
            player_data = new PlayerData(id);

            movement_dict = new Dictionary<Pose, Movement>();
        }

        public string getId()
        {
            return id;
        }

        public int index()
        {
            // start from 0
            return player_index;
        }

        #region 動作偵測紀錄
        /// <summary>
        /// 開始紀錄骨架資訊
        /// </summary>
        public void startRecord()
        {
            is_recording = true;
            record.setStartTime();
        }

        /// <summary>
        /// 停止紀錄骨架資訊
        /// </summary>
        /// <param name="file_id"></param>
        /// <param name="root"></param>
        /// <param name="dir"></param>
        public void stopRecord(string file_id, string root = "", string dir = "")
        {
            Debug.Log(string.Format("[Player] stopRecord | file_id: {0}", file_id));
            is_recording = false;
            record.setEndTime();
            record.save(file_id, root, dir);
            record = new RecordData();
            record.setId(id);
        }

        /// <summary>
        /// 特化為直接根據動作標籤取得正確率並記錄
        /// </summary>
        /// <param name="pose">要紀錄正確率的動作</param>
        public void writeAccuracy(Pose pose)
        {
            record.setAccuracy(getAccuracy(pose));
        }

        public void writeAccuracy(float[] accuracy)
        {
            record.setAccuracy(accuracy);
        }

        public void writeRemark(string remark)
        {
            record.setRemark(remark);
        }

        public void addPosture(Dictionary<HumanBodyBones, Vector3> skeletons, Dictionary<HumanBodyBones, Vector3> rotations)
        {
            record.addPosture(skeletons, rotations);
        }

        /// <summary>
        /// 專為標準紀錄設計
        /// </summary>
        /// <param name="n_sample">分解動作取樣數</param>
        public void samplePosture(int n_sample)
        {
            record.posture_list = Utils.sampleList(record.posture_list, n_sample: n_sample);
        }

        public bool isRecording()
        {
            return is_recording;
        }
        #endregion

        #region movement_dict
        public void setMovement(Pose pose)
        {
            Debug.Log(string.Format("[Player] setMovement(pose: {0})", pose));

            if (!movement_dict.ContainsKey(pose))
            {
                Movement movement = new Movement(pose);

                // 透過 player_data 取得該動作的門檻值(數值因人而異)
                float[] thresholds = player_data.readThreshold(pose);

                //Debug.Log(string.Format("[Player] setMovement | Player {0} load {1} thresholds: {2}",
                //    id, pose, Utils.arrayToString(thresholds)));
                movement.setThreshold(thresholds);

                movement_dict.Add(pose, movement);
            }
            else
            {
                Debug.LogWarning(string.Format("[Player] setMovement | movement_dict has been contain {0}.", pose));
            }
        }

        public Movement getMovement(Pose pose)
        {
            if (movement_dict.ContainsKey(pose))
            {
                return movement_dict[pose];
            }

            Debug.LogError(string.Format("[Player] getMovement | No {0} in movement_dict.", pose));
            return null;
        }

        // 配對成功後，呼叫此函式，將配對過程中的暫存資訊還原
        public void resetMovement(Pose pose)
        {
            if (movement_dict.ContainsKey(pose))
            {
                // 玩家該動作是否通過及正確率等資訊的重置
                movement_dict[pose].resetState();
            }
            else
            {
                Debug.LogWarning(string.Format("[Player] resetMovement | No {0} in movement_dict.", pose));
            }
        } 
        #endregion

        #region 存取配對的動作
        public void setTargetPose(Pose pose)
        {
            target_pose = pose;
        }

        // 紀錄配對完成的動作
        public void setMatchedPose(Pose pose)
        {
            matched_pose = pose;
        }

        public Pose getTargetPose()
        {
            return target_pose;
        }

        /// <summary>
        /// 提供偵測腳本判斷是否已經偵測完成，以及事後根據配對到的動作給予回饋的函式，因為現在大多改成事件監聽，無法配對成功後馬上將結果回傳，需要有變數將結果存起來
        /// </summary>
        /// <returns>已配對到的動作</returns>
        public Pose getMatchedPose()
        {
            return matched_pose;
        }

        public void resetMatchedPose()
        {
            matched_pose = Pose.None;
        }
        #endregion
        #endregion

        #region Movement
        // 紀錄正確率的最高值
        public void setHighestAccuracy(Pose pose, int index, float value)
        {
            try
            {
                // 新數值較大才更新                
                if (value > movement_dict[pose].getAccuracy(index: index))
                {
                    movement_dict[pose].setHighestAccuracy(index: index, value: value);
                }
            }
            catch (IndexOutOfRangeException iooe)
            {
                Debug.LogError(string.Format("[Player] setHighestAccuracy | {0}", iooe.Message));
            }
        }

        public float[] getAccuracy(Pose pose)
        {
            return movement_dict[pose].getAccuracy();
        }

        public float getAccuracy(Pose pose, int index)
        {
            return movement_dict[pose].getAccuracy(index);
        }

        /// <summary>
        /// 使用前一次通過時的正確率，作為下一次的門檻初始值
        /// </summary>
        /// <param name="pose">要設置門檻值的動作</param>
        public void setThreshold(Pose pose)
        {
            movement_dict[pose].setThreshold(getAccuracy(pose));
        }

        public void setThreshold(Pose pose, float[] thresholds, int digits = -1)
        {
            if(digits >= 0)
            {
                movement_dict[pose].setThreshold(thresholds, digits: digits);
            }
            else
            {
                movement_dict[pose].setThreshold(thresholds);
            }            
        }

        public void modifyThreshold(Pose pose, int index, float acc, int optimization = 0)
        {
            movement_dict[pose].setThreshold(index: index, acc: acc, optimization: optimization);
        }

        public void modifyAllThreshold(Pose pose, int optimization = 0)
        {
            int i, len = ConfigData.n_posture;

            // accuracys: 取得該動作的最高正確率
            float[] accuracys = getAccuracy(pose);

            for (i = 0; i < len; i++)
            {
                modifyThreshold(pose: pose, index: i, acc: accuracys[i], optimization: optimization);
            }

            Debug.Log(string.Format("[Player] modifyAllThreshold | Finish modification of {0}\naccuracy: {1}\nthreshold: {2}",
                pose, Utils.arrayToString(getAccuracy(pose)), Utils.arrayToString(getThreshold(pose))));
        }

        public void modifyThreshold(Pose pose, int optimization = 0)
        {
            Debug.Log(string.Format("[Player] modifyThreshold(pose: {0}, optimization: {1})", pose, optimization));
            StartCoroutine(modifyThresholdCoroutine(pose, optimization));
        }

        public IEnumerator modifyThresholdCoroutine(Pose pose, int optimization = 0)
        {
            int i, len = ConfigData.n_posture;

            // accuracys: 取得該動作的最高正確率
            float[] accuracys = getAccuracy(pose);

            for (i = 0; i < len; i++)
            {                
                modifyThreshold(pose: pose, index: i, acc: accuracys[i], optimization: optimization);
            }
            yield return null;

            Debug.Log(string.Format("[Player] modifyThresholdCoroutine | Finish modification of {0}\naccuracy: {1}\nthreshold: {2}", 
                pose, Utils.arrayToString(getAccuracy(pose)), Utils.arrayToString(getThreshold(pose))));
        }

        public float[] getThreshold(Pose pose)
        {
            return movement_dict[pose].getThreshold();
        }

        public float getThreshold(Pose pose, int index)
        {
            return movement_dict[pose].getThreshold(index);
        }

        public void setMatched(Pose pose, int index, bool status)
        {
            movement_dict[pose].setMatched(index: index, status: status);
        }

        public void setAddtionalMatched(Pose pose, bool is_matched)
        {
            movement_dict[pose].setAddtionalMatched(is_matched: is_matched);
        }

        public bool isMatched(Pose pose)
        {
            return movement_dict[pose].isMatched();
        }

        public bool isMatched(Pose pose, int index)
        {
            return movement_dict[pose].isMatched(index: index);
        }
        #endregion

        #region PlayerData: write/read 為 PlayerData 特化之 set/get，和相似功能的其他函式做區分
        public void writeGameStage(GameStage game_stage)
        {
            player_data.setGameStage(game_stage);

            // 動作偵測紀錄
            record.setStage(game_stage);
        }

        public void writeThreshold(Pose pose)
        {
            // 動作偵測紀錄，用於紀錄玩家遊戲過程
            record.setPose(pose);

            float[] values = getAccuracy(pose);
            record.setThreshold(values);
            player_data.setThresholds(pose, values);
        }

        public void writeThreshold(Pose pose, float[] thres)
        {
            // 動作偵測紀錄，用於紀錄玩家遊戲過程
            record.setPose(pose);
            record.setThreshold(thres);

            if(thres != null)
            {
                player_data.setThresholds(pose, thres);
            }
        }

        public float[] readThreshold(Pose pose)
        {
            return player_data.readThreshold(pose);
        }

        /// <summary>
        /// 儲存玩家數據(PlayerData)
        /// </summary>
        public void save()
        {
            Debug.Log(string.Format("[Player] save"));
            player_data.save();
        }
        #endregion

        #region 玩家位移
        public void resetInitPos()
        {
            init_pos = model_helper.transform.position;
        }

        public Vector3? getInitPos()
        {
            return init_pos;
        }

        public float getDistanceX()
        {
            // 相對於初始位置的移動(向量)
            Vector3 vector3 = model_helper.transform.position - (Vector3)init_pos;
            float distance = Math.Abs(vector3.x);
            return distance;
        }

        public float getDistanceY()
        {
            // 相對於初始位置的移動(向量)
            Vector3 vector3 = model_helper.transform.position - (Vector3)init_pos;
            float distance = Math.Abs(vector3.y);
            return distance;
        }

        public float getDistanceZ()
        {
            // 相對於初始位置的移動(向量)
            Vector3 vector3 = model_helper.transform.position - (Vector3)init_pos;
            float distance = Math.Abs(vector3.z);
            return distance;
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
    }
}
