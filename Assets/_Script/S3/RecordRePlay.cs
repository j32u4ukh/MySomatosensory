using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace S3
{
    public class RecordRePlay : MonoBehaviour
    {
        public enum Mode
        {
            Record,
            RePlay,
            Stop
        }

        public Player player;

        // 狀態? 預設為：記錄
        public Mode mode = Mode.Stop;

        // 要錄製或重播的動作
        public DetectSkeleton pose = DetectSkeleton.None;

        // 路徑
        string root, dir, path;
        string[] files;
        public int replay_index;

        // 藉由 GameManager 和 RecordData 進行錄製動作
        GameManager gm;
        RecordData record;

        // 用於存取骨架資訊
        List<Dictionary<HumanBodyBones, Vector3>> skeletons_list, rotations_list;
        Dictionary<HumanBodyBones, Vector3> skeletons, rotations;
        HumanBodyBones humanBodyBone;
        Transform bone;
        Vector3 vector3;
        int frame = 0, frame_number, bone_index, bones_number;

        // Start is called before the first frame update
        void Start()
        {
            root = Path.Combine(Application.streamingAssetsPath, "MovementData");
            dir = Path.Combine(root, pose.ToString());

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if(mode == Mode.RePlay)
            {
                files = Directory.GetFiles(dir);

                if(replay_index > files.Length)
                {
                    replay_index = files.Length - 1;
                }               

                path = files[replay_index];
            }

            gm = GetComponent<GameManager>();
            gm.registPlayers(player);
            print(string.Format("file_id: {0}", gm.file_id));

            switch (mode)
            {
                case Mode.Record:
                    record = new RecordData();
                    record.setType(pose);
                    record.setStage("Record");
                    record.setStartTime();
                    mode = Mode.Stop;
                    break;
                case Mode.RePlay:
                    record = RecordData.loadRecordData(path);
                    record.setType(pose);
                    record.setStage("Record");
                    record.setStartTime();

                    skeletons_list = record.getSkeletonsList();
                    rotations_list = record.getRotationsList();
                    bones_number = player.getBonesNumber();
                    frame_number = skeletons_list.Count;
                    break;
            }
        }

        // Update is called once per frame
        void Update()
        {
            switch (mode)
            {
                case Mode.Record:
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        mode = Mode.Stop;
                        gm.stopRecord();
                        gm.save(root, pose.ToString());
                    }
                    break;
                case Mode.RePlay:
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

                            bone = player.getBoneTransform(bone_index);

                            vector3 = skeletons[humanBodyBone];
                            //print(humanBodyBone + " position: " + vector3);
                            bone.position = vector3;

                            vector3 = rotations[humanBodyBone];
                            //print(humanBodyBone + " rotation: " + vector3);
                            bone.rotation = Quaternion.Euler(vector3);

                        }

                        frame++;
                    }
                    else
                    {
                        print(string.Format("Replay finished at {0} frame.", frame));
                    }
                    
                    break;
                default:
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        mode = Mode.Record;
                        gm.startRecord();
                    }
                    break;
            }
        }
    }
}
