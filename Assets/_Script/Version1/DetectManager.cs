using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System;

public class DetectManager : MonoBehaviour {
    public PoseModelHelper modelHelper;
    Dictionary<int, HumanBodyBones> index2BodyMap;

    #region 動作偵測
    // 用於選擇偵測何種動作
    public DetectSkeleton detectSkeleton = DetectSkeleton.None;

    // 各個動作 是否通過
    Dictionary<DetectSkeleton, bool[]> matchMap;
    // 各個動作 正確率
    public Dictionary<DetectSkeleton, float[]> accuracyMap;
    // 各個動作 門檻
    public Dictionary<DetectSkeleton, float[]> thresholdsMap;
    // 各個動作 標準模型
    public Dictionary<DetectSkeleton, PoseModelHelper[]> poseModelMap;
    // 各個動作 比對關節
    Dictionary<DetectSkeleton, List<HumanBodyBones>> comparingPartsMap;

    #region 比對標準
    public PoseModelHelper[] poseModelHelpers;
    public List<HumanBodyBones> comparingParts;
    // 門檻值數量要與模型數量相同
    float[] handsUpThreshold = new float[] { 0.9f, 0.9f };
    #endregion

    #endregion

    // 偵測結果呈現
    string pose_type = "", message = "";

    private void Awake()
    {
        // 初始化 各個動作 標準模型
        poseModelMap = new Dictionary<DetectSkeleton, PoseModelHelper[]>();
        // 初始化 各個動作 比對關節
        comparingPartsMap = new Dictionary<DetectSkeleton, List<HumanBodyBones>>();
        // 初始化 各個動作 分解動作門檻值
        thresholdsMap = new Dictionary<DetectSkeleton, float[]>();

        init(DetectSkeleton.PutHandsUp, poseModelHelpers, comparingParts, handsUpThreshold);

        resetState();
    }

    // Use this for initialization
    void Start () {
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
            default:
                message = "";
                break;
        }
    }

    private void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        pose_type = string.Format("{0}", detectSkeleton);
        GUILayout.Label(pose_type + "\n" + message);
    }

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

    // 比對動作 + 額外條件
    void compareMovement(DetectSkeleton check_pose, int num = 0, bool additional_condition = true) {        
        int len = poseModelMap.ContainsKey(check_pose) ? poseModelMap[check_pose].Length : 0;
        int i;
        PoseModelHelper[] poseModels = poseModelMap[check_pose];
        float[] thresholds = thresholdsMap[check_pose];

        float acc;
        for (i = 0; i < len; i++)
        {
            acc = getAccuracy(poseModels[i], comparingPartsMap[check_pose]);
            message = string.Format("Accuracy rate: {0:F4}, threshold: {1:F4} ", acc, thresholds[i]);

            // 記錄各個分解動作的最高值
            accuracyMap[check_pose][i] = Mathf.Max(acc, accuracyMap[check_pose][i]);
            // 正確率超過門檻值，則 matchMap 記錄通過
            if (acc > thresholds[i])
            {                
                matchMap[check_pose][i] = true;
            }
        }

        // 當所有動作皆通過                       附加額外通關條件
        if (compare(check_pose) && additional_condition)
        {
            // 任一動作完成偵測，停止偵測
            detectSkeleton = DetectSkeleton.None;

        }
    }

    public DetectSkeleton? whichOnePass(DetectSkeleton detects) {
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
        int len = poseModelMap[skeleton].Length, i;
        for (i = 0; i < len; i++)
        {
            _pass &= matchMap[skeleton][i];
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
        // 取得各個動作的幾何平均
        for (index = 0; index < length; index++)
        {
            skeleton = detects[index];
            len = poseModelMap[skeleton].Length;
            mean = 1f;
            for (i = 0; i < len; i++)
            {
                mean *= accuracyMap[skeleton][i];
            }
            mean = Mathf.Pow(mean, (float) 1/len);
            // 比較幾何平均大小，傳回最大的那個動作
            if (mean > max_mean)
            {
                mean = max_mean;
                max_index = index;
            }
        }

        return detects[max_index];
    }

    void resetState() {
        // 初始化各個動作，是否通過與正確率
        Array array = Enum.GetValues(typeof(DetectSkeleton));
        matchMap = new Dictionary<DetectSkeleton, bool[]>();
        accuracyMap = new Dictionary<DetectSkeleton, float[]>();
        DetectSkeleton skeleton;

        for (int i = 0; i < array.Length; i++)
        {
            skeleton = (DetectSkeleton)array.GetValue(i);
            matchMap.Add(skeleton, new bool[10]);
            accuracyMap.Add(skeleton, new float[10]);

        }
    }

    void init(DetectSkeleton detect, PoseModelHelper[] poseModelHelpers, List<HumanBodyBones> comparingParts, float[] thresholds)
    {
        poseModelMap.Add(detect, poseModelHelpers);
        comparingPartsMap.Add(detect, comparingParts);
        thresholdsMap.Add(detect, thresholds);
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
