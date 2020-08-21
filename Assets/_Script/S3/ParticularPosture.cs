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
        public Player model1, model2, model3;
        Player[] models;
        int model_idx, n_model;

        // 多來源的第 posture_idx 幀 Posture
        List<Posture> postures;
        int posture_idx;

        #region setModelPosture
        HumanBodyBones humanBodyBone;
        int bone_index, bones_number;
        #endregion

        #region getAccuracy
        Transform bone;
        Vector3 vector3;

        FloatList acc_list;
        float acc;
        #endregion

        // Posture
        Movement movement;
        int n_posture;

        // Start is called before the first frame update
        void Start()
        {
            player.setId("9527");            
            player.loadData();

            bones_number = player.getBonesNumber();
            movement = player.getMovement(pose);

            if(movement == null)
            {
                print("movement is null.");
            }

            //multi_postures = movement.getMultiPosture();

            n_posture = movement.getPostureNumber();
            posture_idx = 0;

            models = new Player[]{ model1, model2, model3 };
            n_model = models.Length;

            acc_list = new FloatList();
        }

        // Update is called once per frame
        void Update()
        {
            // 有序姿勢比對
            if (posture_idx < n_posture)
            {
                postures = movement.getPostures(posture_idx);
                acc_list.clear();

                // 多標準共同衡量正確率
                for (model_idx = 0; model_idx < n_model; model_idx++)
                {
                    acc_list.add(getAccuracy(player, postures[model_idx]));
                    //setModelPosture(models[model_idx], postures[model_idx]);
                }

                acc = acc_list.geometricMean();
                if (acc > 0.7f)
                {
                    posture_idx++;
                    print(string.Format("{0} {1}", posture_idx, acc_list.ToString()));
                    print(string.Format("mean: {0}, geometricMean: {1}", acc_list.mean(), acc_list.geometricMean()));
                }
            }
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                posture_idx++;

                if (posture_idx < n_posture)
                {
                    postures = movement.getPostures(posture_idx);
                    
                    for(model_idx = 0; model_idx < n_model; model_idx++)
                    {
                        setModelPosture(models[model_idx], postures[model_idx]);
                    }
                }
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 50;
            GUILayout.Label(string.Format("Frame: {0}/{1}\nAccuracy: {2:F4}\nthresholds: {3}",
                posture_idx + 1,
                movement.getPostureNumber(),
                acc,
                movement.getThreshold(posture_idx)),
                GUILayout.Width(500));
        }

        private void OnDestroy()
        {

        }

        float getAccuracy(Player player, Posture posture)
        {
            List<HumanBodyBones> comparing_parts = player.getComparingParts(pose);
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
                    s1 = posture.getBonePosition(bone);
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
                    s2 = posture.getBonePosition(bone);
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

        void setModelPosture(Player model, Posture posture)
        {
            //print("setModelPosture");

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

                vector3 = posture.getBonePosition(humanBodyBone) + (Vector3)model.getInitPos();
                bone.position = vector3;

                vector3 = posture.getBoneRotation(humanBodyBone);
                bone.rotation = Quaternion.Euler(vector3);

            }
        }

    }
}
