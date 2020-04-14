using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public struct Posture
    {
        public List<HumanBodyBones> comparing_parts;
        public Dictionary<HumanBodyBones, Vector3> skeletons;
        public bool contain(HumanBodyBones bone) {
            return skeletons.ContainsKey(bone);
        }
        public Vector3 getBoneTransform(HumanBodyBones bone) {
            return skeletons[bone];
        }
    }

    public class Movement : MonoBehaviour
    {
        [Header("動作類型")]
        [SerializeField] private DetectSkeleton detect_skeleton;

        [Header("分解動作模型")]
        [SerializeField] private PoseModelHelper[] model_helpers;

        [Header("比對關節")]
        [SerializeField] private List<HumanBodyBones> comparing_parts;        

        [Header("是否有額外條件")]
        [SerializeField] private bool has_additional_condition;

        // 門檻值
        private float[] thresholds;

        // 正確率
        private float[] accuracys;

        // 動作數量
        private int movement_number;

        // 門檻數量
        private int threshold_number;

        // 是否通過
        private bool[] is_matched;

        private void Awake()
        {
            // 模型數量
            movement_number = model_helpers.Length;

            if (has_additional_condition)
            {
                // 若有額外條件，
                threshold_number = movement_number + 1;
            }
            else
            {
                threshold_number = movement_number;
            }

            resetState();
        }

        public int getMovementNumber()
        {
            return movement_number;
        }

        public int getThresholdNumber()
        {   
            return threshold_number;
        }

        public void setThresholds(float[] _thresholds)
        {
            int _length = _thresholds.Length;
            thresholds = new float[_length];
            for(int i = 0; i < _length; i++)
            {
                thresholds[i] = _thresholds[i];
            }

            if (has_additional_condition)
            {
                // 額外條件：移動距離 是否達到要求距離
                thresholds[_length - 1] = 1f;
            }
        }

        public float[] getThresholds()
        {
            return thresholds;
        }

        public float getThreshold(int index)
        {
            try
            {
                return thresholds[index];
            }
            catch (IndexOutOfRangeException)
            {
                return 0f;
            }
        }

        public void setAccuracy(int index, float value)
        {
            try
            {
                accuracys[index] = value;
            }
            catch (IndexOutOfRangeException)
            {

            }
        }

        public float[] getAccuracy()
        {
            return accuracys;
        }

        public float getAccuracy(int index)
        {
            try
            {
                return accuracys[index];
            }
            catch (IndexOutOfRangeException)
            {
                return 0f;
            }
        }

        public DetectSkeleton getMovementType()
        {
            return detect_skeleton;
        }

        public PoseModelHelper[] getModels()
        {
            return model_helpers;
        }

        public List<HumanBodyBones> getComparingParts()
        {
            return comparing_parts;
        }

        public bool hasAddtionalCondition()
        {
            return has_additional_condition;
        }

        public bool isMatched(int index)
        {
            return is_matched[index];
        }

        public void setMatched(int index, bool status)
        {
            is_matched[index] = status;
        }

        public void resetState()
        {
            // 是否通過
            is_matched = new bool[threshold_number];

            // 初始化正確率
            accuracys = new float[threshold_number];
        }
    }
}
