using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace S3
{
    public class ParticularPosture : MonoBehaviour
    {
        public Pose pose = Pose.None;
        public Player player;
        public Player model;

        // 路徑
        string root, dir, path;
        string[] files;
        public int replay_index;
        public int posture_index = 0;
        public List<HumanBodyBones> comparingParts;

        // 用於存取骨架資訊
        RecordData record;
        List<Dictionary<HumanBodyBones, Vector3>> skeletons_list, rotations_list;
        Dictionary<HumanBodyBones, Vector3> skeletons, rotations;
        HumanBodyBones humanBodyBone;
        Transform bone;
        Vector3 vector3;
        int frame = 0, frame_number, bone_index, bones_number;

        float acc, max_acc = 0f;

        // Posture
        Posture posture;

        // Start is called before the first frame update
        void Start()
        {
            bones_number = player.getBonesNumber();

            root = Path.Combine(Application.streamingAssetsPath, "MovementData");
            dir = Path.Combine(root, pose.ToString());
            files = Directory.GetFiles(dir);

            if (replay_index > files.Length / 2)
            {
                replay_index = files.Length / 2 - 1;
            }

            path = files[replay_index * 2];
            print(string.Format("path: {0}", path));

            record = RecordData.loadRecordData(path);
            skeletons_list = record.getSkeletonsList();
            rotations_list = record.getRotationsList();

            List<int> indexs = Utils.sampleIndex(skeletons_list.Count, 5);
            skeletons_list = Utils.sampleList(skeletons_list, indexs);
            rotations_list = Utils.sampleList(rotations_list, indexs);
            frame_number = skeletons_list.Count;
            print(string.Format("frame_number: {0}", frame_number));

            setModelPosture();
        }

        // Update is called once per frame
        void Update()
        {
            acc = getAccuracy(player, posture);
            max_acc = Mathf.Max(max_acc, acc);

            if (acc > 0.75f)
            {
                frame++;
                print(string.Format("Current frame: {0}, Accuracy: {1:F4}", frame, acc));

                if (frame < frame_number)
                {
                    setModelPosture();
                }
                else
                {
                    frame = frame_number;
                }
            }
        }

        private void OnGUI()
        {            
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 50;
            GUILayout.Label(string.Format("Frame: {0}/{1}\nAccuracy: {2:F4}", frame + 1, frame_number, acc));
        }

        private void OnDestroy()
        {
            print(string.Format("max_acc: {0:F4}", max_acc));
        }

        float getAccuracy(Player player, List<HumanBodyBones> comparingParts)
        {
            int i, index, len = comparingParts.Count;
            Transform p1, p2, s1, s2;
            Vector3 playerBone, standardBone;

            float diff, total_diff = 0f;

            for (i = 0; i < len; i++)
            {
                //index = PoseModelHelper.bone2IndexMap[comparingParts[i]];
                index = PoseModelHelper.boneToIndex(comparingParts[i]);

                if ((p1 = player.getBoneTransform(index)) == null)
                {
                    continue;
                }
                if ((s1 = model.getBoneTransform(index)) == null)
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

                if ((p2 = player.getBoneTransform(index)) == null)
                {
                    continue;
                }
                if ((s2 = model.getBoneTransform(index)) == null)
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

        float getAccuracy(Player player, Posture posture)
        {
            List<HumanBodyBones> comparing_parts = posture.comparing_parts;
            HumanBodyBones bone;
            Vector3 player_vector, standrad_vector;
            Vector3 s1, s2, p1, p2;


            int i, len = comparing_parts.Count;
            Transform trans;

            float diff, total_diff = 0f;

            for (i = 0; i < len; i++)
            {
                bone = comparing_parts[i];

                if ((trans = player.getBoneTransform(bone)) == null)
                {
                    continue;
                }
                else
                {
                    p1 = trans.position;
                }

                if (!posture.contain(bone))
                {
                    continue;
                }
                else
                {
                    s1 = posture.getBoneTransform(bone);
                }

                if ((i + 1) >= len)
                {
                    bone = comparing_parts[0];
                }
                else
                {
                    bone = comparing_parts[i + 1];
                }

                if ((trans = player.getBoneTransform(bone)) == null)
                {
                    continue;
                }
                else
                {
                    p2 = trans.position;
                }

                if (!posture.contain(bone))
                {
                    continue;
                }
                else
                {
                    s2 = posture.getBoneTransform(bone);
                }

                //取得玩家與標準模型 目前節點(jointType)的向量
                player_vector = (p2 - p1).normalized;
                standrad_vector = (s2 - s1).normalized;

                //計算玩家骨架 與 姿勢骨架角度差距
                diff = Vector3.Angle(player_vector, standrad_vector);
                if (diff > 90f)
                {
                    diff = 90f;
                }
                total_diff += diff / 90f;
            }

            total_diff /= len;

            return 1f - total_diff;
        }

        void setModelPosture()
        {
            skeletons = skeletons_list[frame];
            rotations = rotations_list[frame];

            posture = new Posture
            {
                comparing_parts = comparingParts,
                skeletons = skeletons
            };

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

                bone = model.getBoneTransform(bone_index);

                vector3 = skeletons[humanBodyBone];
                bone.position = vector3;

                vector3 = rotations[humanBodyBone];
                bone.rotation = Quaternion.Euler(vector3);

            }
        }

    }
}
