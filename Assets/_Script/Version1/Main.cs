using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour {
    public PoseModelHelper modelHelper;
    Dictionary<int, HumanBodyBones> index2BodyMap;
    

    DetectManager detect_manager;

    #region 紀錄遊戲數據
    RecordData recordData;
    string path;
    string file_id;

    #region 紀錄骨架位置
    Transform bone;
    Vector3 vector3;
    Dictionary<HumanBodyBones, Vector3> skeletons;
    Dictionary<HumanBodyBones, Vector3> rotations;
    bool startRecodeSkeleton = false;
    int len;
    //float time = 3f;
    //public Text show_time;    
    #endregion

    #endregion

    // Use this for initialization
    void Start () {
        index2BodyMap = PoseModelHelper.boneIndex2MecanimMap;
        len = modelHelper.GetBoneTransformCount();
        recordData = new RecordData();
        detect_manager = GetComponent<DetectManager>();
        file_id = DateTime.Now.ToString("HH-mm-ss-ffff");

        StartCoroutine(dataOutput());
    }
	
	// Update is called once per frame
	void Update () {
                
    }

    private void FixedUpdate()
    {
        if (startRecodeSkeleton)
        {
            // 每次紀錄 1 幀，所有關節位置，直到 startRecodeSkeleton = false，最終會記錄許多幀
            skeletons = new Dictionary<HumanBodyBones, Vector3>();
            rotations = new Dictionary<HumanBodyBones, Vector3>();

            for (int i = 0; i < len; i++)
            {
                if ((bone = modelHelper.GetBoneTransform(i)) == null)
                {
                    continue;
                }
                vector3 = bone.transform.position;
                skeletons.Add(index2BodyMap[i], vector3);

                vector3 = bone.transform.rotation.eulerAngles;
                rotations.Add(index2BodyMap[i], vector3);

            }
            recordData.addSkeletons(skeletons);
            recordData.addRotations(rotations);
        }
    }

    IEnumerator dataOutput()
    {
        yield return new WaitForSeconds(2f);
        startMatch(DetectSkeleton.PutHandsUp);
        DetectSkeleton? result;
        while ((result = detect_manager.whichOnePass(DetectSkeleton.PutHandsUp)) == null)
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }
        endMatch(DetectSkeleton.PutHandsUp);

        // 一個遊戲的所有紀錄皆寫完後，加上後綴"_done"，告訴其他程式已經可以上傳
        RecordData.finishWriting(path);
    }

    void startMatch(DetectSkeleton pose)
    {
        print("startMatch");
        startRecodeSkeleton = true;
        detect_manager.detectSkeleton = pose;
        recordData.setStage("Test");
        recordData.setStartTime();

    }

    DetectSkeleton endMatch(DetectSkeleton pose)
    {
        print("endMatch");
        startRecodeSkeleton = false;
        recordData.setEndTime();

        DetectSkeleton? result;
        DetectSkeleton detect;
        // 判斷是單一動作還是多個動作
        switch (pose)
        {
            case DetectSkeleton.SingleFootJump:
                result = detect_manager.whichOnePass(pose);                
                if(result == null)
                {
                    // 如果都失敗
                    detect = detect_manager.thisOneFailed(DetectSkeleton.SingleFootJump);
                }
                else
                {
                    // 如果其中一個成功
                    detect = (DetectSkeleton)result;
                }
                
                break;
            default:
                detect = pose;
                break;
        }

        // detect：最後認定玩家做的動作
        recordData.setType(detect);
        recordData.setThreshold(detect_manager.thresholdsMap[detect]);
        int length = detect_manager.poseModelMap[detect].Length;
        recordData.setAccuracy(DetectManager.sliceArray(detect_manager.accuracyMap[detect], 0, length));

        // 取消偵測
        detect_manager.detectSkeleton = DetectSkeleton.None;
        path = recordData.save(file_id);
        recordData = new RecordData();

        return detect;
    }
    

}
