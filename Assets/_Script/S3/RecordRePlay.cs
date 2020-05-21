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
        public Pose pose = Pose.None;

        // 路徑
        string root, dir, path;
        string[] files;
        public int replay_index;

        // 藉由 GameManager 和 RecordData 進行錄製動作
        GameManager gm;
        RecordData record;

        // 用於存取骨架資訊
        List<Posture> posture_list;
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

                if(replay_index > files.Length / 2)
                {
                    replay_index = files.Length / 2 - 1;
                }

                path = files[replay_index * 2];
                print(string.Format("path: {0}", path));
            }

            gm = GetComponent<GameManager>();
            player.setId("9527");
            player.loadData();

            gm.registPlayers(player);
            print(string.Format("file_id: {0}", gm.file_id));

            switch (mode)
            {
                case Mode.Record:
                    gm.setPose(pose);
                    gm.setStage(GameStage.Test);
                    gm.setStartTime();
                    mode = Mode.Stop;
                    break;
                case Mode.RePlay:
                    record = RecordData.loadRecordData(path);
                    posture_list = record.getPostureList();
                    //posture_list = Utils.sampleList(posture_list, 30);
                    print(string.Format("skeletons_list: {0}", posture_list.Count));

                    bones_number = player.getBonesNumber();
                    frame_number = posture_list.Count;
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
                        print(string.Format("Frame {0}", frame));
                        skeletons = record.getSkeletons(frame);
                        rotations = record.getRotations(frame);

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
