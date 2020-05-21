using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace Somatosensory2
{
    public class DetectManager : MonoBehaviour
    {
        public PoseModelHelper modelHelper;
        Dictionary<int, HumanBodyBones> index2BodyMap;

        #region 動作偵測
        // 用於選擇偵測何種動作
        public DetectSkeleton detectSkeleton = DetectSkeleton.None;

        // 各個動作 的 動作物件
        public Movement[] movements;

        // 動作名稱 與 動作物件 的字典
        public Dictionary<DetectSkeleton, Movement> movement_map;
        #endregion

        // 偵測結果呈現
        string pose_type = "", message = "";
        Vector3? init_pos;
        float max_z_distance;

        #region Life cycle
        // Use this for initialization
        void Start()
        {
            init_pos = null;
            max_z_distance = 0f;
            movement_map = new Dictionary<DetectSkeleton, Movement>();
            DetectSkeleton movement_type;
            foreach (Movement m in movements)
            {
                movement_type = m.getMovementType();
                movement_map.Add(movement_type, m);
                print(string.Format("Add {0} into movement_map", movement_type));
            }

            initMovement();
            resetState();

            index2BodyMap = PoseModelHelper.boneIndex2MecanimMap;
        }

        // Update is called once per frame
        void Update()
        {
            switch (detectSkeleton)
            {
                case DetectSkeleton.PutHandsUp:
                    compareMovement(DetectSkeleton.PutHandsUp);
                    break;

                case DetectSkeleton.CrossJump:
                    if(init_pos == null)
                    {
                        init_pos = modelHelper.transform.position;
                    }

                    float z_distance = Math.Abs(modelHelper.transform.position.z - ((Vector3)init_pos).z);
                    float z_threshold = 0.2f;
                    max_z_distance = Mathf.Max(max_z_distance, z_distance);
                    print(string.Format("z_distance: {0:F4}, max_z_distance: {1:F4}",
                        z_distance, max_z_distance));
                    
                    float additionnal_acc = max_z_distance / z_threshold;
                    print(string.Format("z_threshold: {0:F4}, additionnal_acc: {1:F4}",
                        z_threshold, additionnal_acc));

                    compareMovement(DetectSkeleton.CrossJump, additionnal_acc);
                    break;
                default:
                    message = "";
                    break;
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 40;
            pose_type = string.Format("{0}", detectSkeleton);
            GUILayout.Label(pose_type + "\n" + message);
        }
        #endregion

        // ====================
        // 取得單一姿勢正確率
        float getAccuracy(PoseModelHelper poseModelHelper, List<HumanBodyBones> comparingParts)
        {
            int i, index, len = comparingParts.Count;
            Transform p1, p2, s1, s2;
            Vector3 playerBone, standardBone;

            float diff = 0f, total_diff = 0f;

            for (i = 0; i < len; i++)
            {
                index = PoseModelHelper.bone2IndexMap[comparingParts[i]];
                if ((p1 = modelHelper.GetBoneTransform(index)) == null)
                {
                    continue;
                }
                if ((s1 = poseModelHelper.GetBoneTransform(index)) == null)
                {
                    continue;
                }

                if ((i + 1) >= len)
                {
                    index = PoseModelHelper.bone2IndexMap[comparingParts[0]];
                }
                else
                {
                    index = PoseModelHelper.bone2IndexMap[comparingParts[i + 1]];
                }

                if ((p2 = modelHelper.GetBoneTransform(index)) == null)
                {
                    continue;
                }
                if ((s2 = poseModelHelper.GetBoneTransform(index)) == null)
                {
                    continue;
                }

                //取得玩家與標準模型 目前節點(jointType)的向量
                playerBone = (p2.position - p1.position).normalized;
                standardBone = (s2.position - s1.position).normalized;

                //計算玩家骨架 與 姿勢骨架角度差距
                diff = Vector3.Angle(playerBone, standardBone);
                if (diff > 90f)
                {
                    diff = 90f;
                }
                total_diff += diff / 90f;
            }

            total_diff /= len;

            return 1f - total_diff;
        }

        // 比對動作
        void compareMovement(DetectSkeleton check_pose)
        {
            try
            {
                Movement _movement = movement_map[check_pose];
                PoseModelHelper[] poseModels = _movement.getModels();
                float[] thresholds = _movement.getThresholds();
                int i, len = _movement.getMovementNumber();
                float acc;
                for (i = 0; i < len; i++)
                {
                    acc = getAccuracy(poseModels[i], _movement.getComparingParts());
                    message = string.Format("type:{0}[{1}], acc: {2:F4}, threshold: {3:F4} ",
                        check_pose, i, acc, thresholds[i]);
                    print(message);

                    // 記錄各個分解動作的最高值
                    _movement.setAccuracy(i, Mathf.Max(acc, _movement.getAccuracy(i)));

                    // 正確率超過門檻值，則 matchMap 記錄通過
                    if (acc > thresholds[i])
                    {
                        _movement.setMatched(i, true);
                    }
                }

                // 當所有動作皆通過                       
                if (compare(check_pose))
                {
                    // 任一動作完成偵測，停止偵測
                    detectSkeleton = DetectSkeleton.None;
                }
            }
            catch (KeyNotFoundException)
            {
                print(string.Format("No {0} in movement_map", check_pose));
            }
        }

        // 比對動作 + 額外條件
        void compareMovement(DetectSkeleton check_pose, float additional_accuracy)
        {            
            try
            {
                Movement _movement = movement_map[check_pose];
                PoseModelHelper[] poseModels = _movement.getModels();
                float[] thresholds = _movement.getThresholds();
                int i, len = _movement.getMovementNumber();
                float acc;

                // 各個分解動作
                for (i = 0; i < len; i++)
                {
                    acc = getAccuracy(poseModels[i], _movement.getComparingParts());
                    message = string.Format("type:{0}[{1}], acc: {2:F4}, threshold: {3:F4} ",
                        check_pose, i, acc, thresholds[i]);
                    print(message);

                    // 記錄各個分解動作的最高值
                    _movement.setAccuracy(i, Mathf.Max(acc, _movement.getAccuracy(i)));

                    // 正確率超過門檻值，則 matchMap 記錄通過
                    if (acc > thresholds[i])
                    {
                        _movement.setMatched(i, true);
                    }
                }

                // 附加額外通關條件
                if (_movement.hasAddtionalCondition())
                {
                    _movement.setAccuracy(i, Mathf.Max(additional_accuracy, _movement.getAccuracy(i)));
                    print(string.Format("additional_accuracy: {0:F4}", additional_accuracy));

                    if(_movement.getAccuracy(i) > thresholds[i])
                    {
                        _movement.setMatched(i, true);
                    }
                }

                // 當所有動作皆通過                       
                if (compare(check_pose))
                {
                    // 任一動作完成偵測，停止偵測
                    detectSkeleton = DetectSkeleton.None;
                }
            }
            catch (KeyNotFoundException)
            {
                print(string.Format("No {0} in movement_map", check_pose));
            }
        }

        public DetectSkeleton? whichOnePass(DetectSkeleton detects)
        {
            switch (detects)
            {
                case DetectSkeleton.SingleFootJump:
                    return whichPass(DetectSkeleton.SingleLeftFootJump, DetectSkeleton.SingleRightFootJump);
                default:
                    return whichPass(detects);
            }
        }

        DetectSkeleton? whichPass(params DetectSkeleton[] detects)
        {
            int i, numbers = detects.Length;
            DetectSkeleton skeleton;

            for (i = 0; i < numbers; i++)
            {
                skeleton = detects[i];
                if (compare(skeleton))
                {
                    return skeleton;
                }
            }
            return null;
        }

        // 檢查是否所有動作皆通過
        bool compare(DetectSkeleton skeleton)
        {
            bool _pass = true;
            Movement _movement = movement_map[skeleton];
            int len = _movement.getThresholdNumber(), i;
            for (i = 0; i < len; i++)
            {
                _pass &= _movement.isMatched(i);
            }
            return _pass;
        }

        // 處理多動作配對失敗，返回正確率最高的那個動作，推測是做該動作但失敗
        public DetectSkeleton thisOneFailed(DetectSkeleton detects)
        {
            switch (detects)
            {
                case DetectSkeleton.SingleFootJump:
                    return compareAccuracy(DetectSkeleton.SingleLeftFootJump, DetectSkeleton.SingleRightFootJump);
                default:
                    return detects;
            }
        }

        public DetectSkeleton compareAccuracy(params DetectSkeleton[] detects)
        {
            int length = detects.Length, index, i, len, max_index = 0;
            float mean, max_mean = 0f;
            DetectSkeleton skeleton;
            Movement _movement;
            // 取得各個動作的幾何平均
            for (index = 0; index < length; index++)
            {
                skeleton = detects[index];
                _movement = movement_map[skeleton];
                len = _movement.getThresholdNumber();
                mean = 1f;
                for (i = 0; i < len; i++)
                {
                    mean *= _movement.getAccuracy(i);
                }
                mean = Mathf.Pow(mean, (float)1 / len);
                // 比較幾何平均大小，傳回最大的那個動作
                if (mean > max_mean)
                {
                    mean = max_mean;
                    max_index = index;
                }
            }

            return detects[max_index];
        }

        public void resetState()
        {
            // 初始化各個動作，是否通過 與 正確率
            foreach(Movement m in movement_map.Values)
            {
                m.resetState();
            }
        }

        // read GameConfig
        void initMovement()
        {
            // 取得遊戲數據
            Dictionary<string, float[]> threshold_config = GameConfig.loadThreshold();

            Array array = Enum.GetValues(typeof(DetectSkeleton));
            DetectSkeleton skeleton;
            Movement _movement;
            float[] _threshold;

            for (int i = 0; i < array.Length; i++)
            {
                // 取得動作名稱
                skeleton = (DetectSkeleton)array.GetValue(i);

                try
                {
                    // 透過名稱，取得 動作物件
                    _movement = movement_map[skeleton];
                }
                catch (KeyNotFoundException)
                {
                    continue;
                }

                try
                {
                    string key = string.Format("{0}", skeleton);

                    // 透過名稱，取得 門檻值
                    _threshold = threshold_config[key];

                    // 設置門檻值
                    _movement.setThresholds(_threshold);

                    // 有門檻值後，才能確定 正確率 個數
                    _movement.resetState();
                }
                catch (KeyNotFoundException)
                {
                    print(string.Format("Loss threshold of {0}", skeleton));
                    continue;
                }
            }
        }

        public static float[] sliceArray(float[] array, int start, int end)
        {
            int len = end - start, i;
            float[] new_array = new float[len];
            for (i = 0; i < len; i++)
            {
                new_array[i] = array[start + i];
            }
            return new_array;
        }
    }

}