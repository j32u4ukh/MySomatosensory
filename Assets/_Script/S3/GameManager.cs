﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public class GameManager : MonoBehaviour
    {
        List<Player> players;
        List<RecordData> records;
        DetectManager detect_manager;

        #region 紀錄
        [HideInInspector] public string file_id;

        #region 紀錄骨架位置
        bool is_skeleton_recording = false;

        Transform bone;
        Vector3 vector3;
        Dictionary<HumanBodyBones, Vector3> skeletons;
        Dictionary<HumanBodyBones, Vector3> rotations;

        int i, LEN;
        #endregion 
        #endregion

        private void Awake()
        {
            file_id = DateTime.Now.ToString("HH-mm-ss-ffff");
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void FixedUpdate()
        {
            if (is_skeleton_recording)
            {
                print("[GameManager] is_skeleton_recording");

                // 每次紀錄 1 位玩家
                foreach (var player in players)
                {
                    // 每次紀錄 1 幀，所有關節位置，直到 is_skeleton_recording = false，最終會記錄許多幀
                    skeletons = new Dictionary<HumanBodyBones, Vector3>();
                    rotations = new Dictionary<HumanBodyBones, Vector3>();

                    for (i = 0; i < LEN; i++)
                    {
                        if ((bone = player.getBoneTransform(i)) == null)
                        {
                            continue;
                        }
                        vector3 = bone.transform.position;
                        skeletons.Add(player.indexToBones(i), vector3);

                        vector3 = bone.transform.rotation.eulerAngles;
                        rotations.Add(player.indexToBones(i), vector3);

                    }

                    records[player.index()].addSkeletons(skeletons);
                    records[player.index()].addRotations(rotations);
                }
            }
        }

        public void startRecord()
        {
            print("[GameManager] startRecord");
            is_skeleton_recording = true;
        }

        public void stopRecord()
        {
            print("[GameManager] stopRecord");
            is_skeleton_recording = false;
        }

        public void save(string root = "", string dir = "")
        {
            foreach (var player in players)
            {
                records[player.index()].save(file_id, root, dir);
            }
        }

        public void registPlayers(params Player[] player_list)
        {
            players = new List<Player>();
            records = new List<RecordData>();

            foreach (Player player in player_list)
            {
                players.Add(player);
                records.Add(new RecordData());
            }

            LEN = players[0].getBonesNumber();
        }

        public void setDetectManager(DetectManager detect_manager)
        {
            this.detect_manager = detect_manager;
        }

        public void startMatch(DetectSkeleton pose)
        {
            print("startMatch");
            is_skeleton_recording = true;
            detect_manager.pose = pose;

            foreach (Player player in players)
            {
                records[player.index()].setStage("Test");
                records[player.index()].setStartTime();
            }
        }

        public List<DetectSkeleton?> endMatch()
        {
            print("endMatch");
            is_skeleton_recording = false;
            List<DetectSkeleton?> results = new List<DetectSkeleton?>();

            DetectSkeleton pose = detect_manager.pose;
            DetectSkeleton? result;
            DetectSkeleton detect = DetectSkeleton.None;
            foreach (Player player in players) {
                records[player.index()].setEndTime();

                // TODO: DetectManager 直接根據
                // 判斷是單一動作還是多個動作
                switch (pose)
                {
                    case DetectSkeleton.SingleFootJump:
                        //result = detect_manager.whichOnePass(pose);
                        //if (result == null)
                        //{
                        //    // 如果都失敗
                        //    detect = detect_manager.thisOneFailed(DetectSkeleton.SingleFootJump);
                        //}
                        //else
                        //{
                        //    // 如果其中一個成功
                        //    detect = (DetectSkeleton)result;
                        //}

                        break;
                    default:
                        detect = pose;
                        break;
                }

                // detect：最後認定玩家做的動作
                records[player.index()].setType(detect);
                //record_data.setThreshold(detect_manager.thresholdsMap[detect]);
                //int length = detect_manager.poseModelMap[detect].Length;
                //record_data.setAccuracy(DetectManager.sliceArray(detect_manager.accuracyMap[detect], 0, length));

                // 取消偵測
                //detect_manager.detectSkeleton = DetectSkeleton.None;
                //path = recordData.save(file_id);
                records[player.index()] = new RecordData();

                results.Add(detect);
            }

            return results;
        }
    }
}