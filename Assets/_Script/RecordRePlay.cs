using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETLab
{
    public class RecordRePlay : MonoBehaviour
    {
        public enum Mode
        {
            Record,
            RePlay,
            Stop
        }

        public PlayerManager pm;
        public DetectManager dm;

        Player player;
        public string player_id;

        // 狀態? 預設為：記錄
        public Mode mode = Mode.Stop;

        // 要錄製或重播的動作
        public Pose pose = Pose.None;

        // 路徑
        string root, dir;

        public int replay_index;

        // 藉由 GameManager 和 RecordData 進行錄製動作
        //GameManager gm;
        RecordData record;

        // 用於存取骨架資訊
        Dictionary<HumanBodyBones, Vector3> skeletons, rotations;
        HumanBodyBones humanBodyBone;
        Transform bone;
        Vector3 vector3;
        int frame = 0, frame_number, bone_index, bones_number;

        bool has_initialization;

        // Start is called before the first frame update
        void Start()
        {
            root = Path.Combine(Application.streamingAssetsPath, "MovementData");
            pm.init(n_player: 1);
            player = pm.getPlayer(0);
            player.setId(player_id);
            has_initialization = false;
            Debug.Log(string.Format("[RecordRePlay] before initialization"));
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                dir = Path.Combine(root, pose.ToString());

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string path = "";
                if (mode == Mode.RePlay)
                {
                    string[] files = Directory.GetFiles(dir);

                    if (replay_index > files.Length / 2)
                    {
                        replay_index = files.Length / 2 - 1;
                    }

                    path = files[replay_index * 2];
                    Debug.Log(string.Format("[RecordRePlay] path: {0}", path));
                }

                switch (mode)
                {
                    case Mode.Record:
                        player.writeThreshold(pose, thres: null);
                        player.writeGameStage(GameStage.Test);
                        mode = Mode.Stop;
                        Debug.Log(string.Format("[RecordRePlay] Mode.Record | setPose: {0}, setStage: {1}",
                            pose, GameStage.Test));
                        break;
                    case Mode.RePlay:
                        record = RecordData.loadRecordData(path);
                        frame_number = record.getPostureNumber();
                        Debug.Log(string.Format("[RecordRePlay] Mode.RePlay | frame_number: {0}", frame_number));

                        bones_number = player.getBonesNumber();
                        break;
                }

                has_initialization = true;
                Debug.Log(string.Format("[RecordRePlay] Have been initialized."));
            }

            if (has_initialization)
            {
                switch (mode)
                {
                    case Mode.Record:
                        if (Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(0))
                        {
                            mode = Mode.Stop;
                            //gm.stopRecord();
                            //gm.save(root, pose.ToString());
                            dm.stopRecord(player, root, dir);
                            dm.stopRecord(invoke_event: false);
                            has_initialization = false;
                        }
                        break;
                    case Mode.RePlay:
                        if (frame < frame_number)
                        {
                            //print(string.Format("Frame {0}", frame));
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
                            frame = 0;
                            has_initialization = false;
                        }

                        break;
                    default:
                        if (Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(0))
                        {
                            mode = Mode.Record;
                            dm.startRecord();
                        }
                        break;
                }
            }
        }
    }
}
