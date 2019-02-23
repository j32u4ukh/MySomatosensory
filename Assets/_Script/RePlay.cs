using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RePlay : MonoBehaviour {
    public PoseModelHelper modelHelper;

    RecordData recordData;
    string path;
    List<Dictionary<HumanBodyBones, Vector3>> skeletons_list, rotations_list;
    Dictionary<HumanBodyBones, Vector3> skeletons, rotations;
    HumanBodyBones humanBodyBone;
    Transform bone;
    Vector3 vector3;
    int frame = 0, frame_number, bone_index, bones_number;

    // Use this for initialization
    void Start () {
        path = @"C:\Users\etlab\Documents\Somatosensory\Data\9527\2019-02-23\TempScene_14-36-32-8574.txt";
        recordData = RecordData.loadRecordData(path);
        skeletons_list = recordData.getSkeletonsList();
        rotations_list = recordData.getRotationsList();
        bones_number = modelHelper.GetBoneTransformCount();
        frame_number = skeletons_list.Count;

        print("skeletons_list.Count: " + skeletons_list.Count);
        print("rotations_list.Count: " + rotations_list.Count);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate()
    {
        if (frame < frame_number)
        {
            skeletons = skeletons_list[frame];
            rotations = rotations_list[frame];

            for (bone_index = 0; bone_index < bones_number; bone_index++)
            {
                if (!PoseModelHelper.boneIndex2MecanimMap.ContainsKey(bone_index))
                {
                    continue;
                }
                else
                {
                    humanBodyBone = PoseModelHelper.boneIndex2MecanimMap[bone_index];
                }

                bone = modelHelper.GetBoneTransform(bone_index);

                vector3 = skeletons[humanBodyBone];
                print(humanBodyBone + " position: " + vector3);
                bone.position = vector3;

                vector3 = rotations[humanBodyBone];
                print(humanBodyBone + " rotation: " + vector3);
                bone.rotation = Quaternion.Euler(vector3);

            }
        }
        frame++;
    }

    void rePlay(string path)
    {
        recordData = RecordData.loadRecordData(path);
        skeletons_list = recordData.getSkeletonsList();
        frame_number = skeletons_list.Count;
    }
}
