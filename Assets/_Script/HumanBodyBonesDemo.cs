using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine.UI;

public class HumanBodyBonesDemo : MonoBehaviour {
    public GameObject model;
    PoseModelHelper modelHelper;
    Dictionary<int, HumanBodyBones> bodyMap;

    #region 紀錄遊戲數據
    RecordData recordData;
    StreamWriter writer;
    string root, path;

    #region 紀錄骨架位置
    Transform bone;
    Vector3 pos;
    Vector3 x_y_z;
    Dictionary<HumanBodyBones, Vector3> each_bone_pos;
    bool startRecodeSkeleton;
    public Text show_time;
    float time;
    #endregion

    #endregion

    int count = 0, len;

	// Use this for initialization
	void Start () {
        modelHelper = model.GetComponent<PoseModelHelper>();
        bodyMap = PoseModelHelper.boneIndex2MecanimMap;
        len = modelHelper.GetBoneTransformCount();
        // [我的文件] 資料夾
        // C:\Users\etlab\Documents\Somatosensory\Data
        root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Somatosensory\\Data");
        // C:\Users\etlab\Documents\Somatosensory\Data\20190201.txt
        path = Path.Combine(root, string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd")));

        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        time = 3f;
        startRecodeSkeleton = false;

        //StartCoroutine(countTime());
    }
	
	// Update is called once per frame
	void Update () {        
        show_time.text = string.Format("{0}", (int)time);

    }

    private void FixedUpdate()
    {
        //if (startRecodeSkeleton) {
        //    dataOutput();
        //}
        bone = modelHelper.GetBoneTransform(13);
        pos = bone.transform.position;
        Debug.Log(string.Format("Bone : {0}, Position:({1}, {2}. {3})", bodyMap[13], pos.x, pos.y, pos.z));
        
    }

    IEnumerator countTime()
    {
        while (time > 0f)
        {
            time -= Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        startRecodeSkeleton = true;
    }

    void dataOutput() {
        recordData = new RecordData();
        each_bone_pos = new Dictionary<HumanBodyBones, Vector3>();
        recordData.setStage(string.Format("{0}", count));
        recordData.setStartTime();
        //recordData.setThreshold(78);
        //recordData.setAccuracy(87);

        for (int i = 0; i < len; i++)
        {
            if ((bone = modelHelper.GetBoneTransform(i)) == null)
            {
                continue;
            }
            pos = bone.transform.position;
            x_y_z = new Vector3(pos.x, pos.y, pos.z);
            each_bone_pos.Add(bodyMap[i], x_y_z);
            //Debug.Log(string.Format("Bone {0}: {1}, Position:({2}, {3}. {4})", i, bodyMap[i], pos.x, pos.y, pos.z));
        }
        recordData.addSkeletons(each_bone_pos);
        recordData.setEndTime();

        // 檢查檔案是否存在，不存在則建立
        if (!File.Exists(path))
        {
            writer = new FileInfo(path).CreateText();
        }
        else
        {
            writer = new FileInfo(path).AppendText();
        }

        // JsonConvert.SerializeObject 將 recordData 轉換成json格式的字串
        writer.WriteLine(JsonConvert.SerializeObject(recordData));
        writer.Close();
        writer.Dispose();

        startRecodeSkeleton = false;
    }
}
