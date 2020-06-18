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

        public void setPose(Pose pose)
        {
            record.setPose(pose);
        }

        public void setStage(GameStage stage)
        {
            record.setStage(stage);
        }

        // 開始紀錄骨架資訊
        public void startRecord()
        {
            is_recording = true;
            record.setStartTime();
        }

        public void setEndTime()
        {
            record.setEndTime();
        }

        public void setThreshold(float[] threshold)
        {
            record.setThreshold(threshold);
        }

        public void setAccuracy(float[] accuracy)
        {
            record.setAccuracy(accuracy);
        }

        public void setRemark(string remark)
        {
            record.setRemark(remark);
        }

        public void addPosture(Dictionary<HumanBodyBones, Vector3> skeletons, Dictionary<HumanBodyBones, Vector3> rotations)
        {
            record.addPosture(skeletons, rotations);
        }
        
        public void stoptRecord(string file_id, string root = "", string dir = "")
        {
            is_recording = false;
            record.save(file_id, root, dir);
            record = new RecordData();
            record.setId(id);
        }

        public void saveRecordData(string file_id)
        {
            record.save(file_id);
        }

        public bool isRecording()
        {
            return is_recording;
        }

        public void setMovement(Pose pose)
        {
            if (!movement_dict.ContainsKey(pose))
            {
                Movement movement = new Movement(pose);

                // 透過 player_data 取得該動作的門檻值(數值因人而異)
                float[] thresholds = player_data.getThresholds(pose);
                Debug.Log(string.Format("[Player] setMovement | Player {0} load {1} thresholds: {2}",
                    id, pose, Utils.arrayToString(thresholds)));
                movement.setThresholds(thresholds);

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
                movement_dict[pose].resetState();
            }
            else
            {
                Debug.LogWarning(string.Format("[Player] resetMovement | No {0} in movement_dict.", pose));
            }
        }

        #region 存取配對完成的動作
        // 紀錄配對完成的動作
        public void setMatchedPose(Pose pose)
        {
            matched_pose = pose;
        }

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

        #region PlayerData
        public void setGameStage(GameStage game_stage)
        {
            player_data.setGameStage(game_stage);
        }

        public void setThresholds(Pose pose, float[] thres)
        {
            player_data.setThresholds(pose, thres);
        }

        public void setThreshold(Pose pose, int index, float thres)
        {
            player_data.setThreshold(pose, index, thres);
        }

        public float[] getThresholds(Pose pose)
        {
            return player_data.getThresholds(pose);
        }

        public void save()
        {
            Debug.Log(string.Format("[Player] save"));
            player_data.save();
        }
        #endregion

        #region 計算玩家位移
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
