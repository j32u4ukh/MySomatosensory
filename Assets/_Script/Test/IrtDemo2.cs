﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ETLab
{
    public class IrtDemo2 : MonoBehaviour
    {
        public DetectManager dm;
        public PlayerManager pm;
        [Header("偵測動作類型")]
        public Pose pose = Pose.RaiseTwoHands;

        string gui = "";
        float modify_milli = 3.0f;
        float interval_milli = 0.1f;

        // ====================================================================================================
        // 音效管理
        public AudioManager audio_manager;

        List<string> questions;

        #region UI
        // 回饋圖片
        public GameObject success_image;
        public GameObject fail_image;

        public Image question_image;
        public Text question_text;

        // 告訴玩家目前達成次數
        public Image ui_count;

        // 呈現最後得分
        public ScoreBoard score_borad;
        public Image hint;
        #endregion

        #region Game Config
        // 總共有幾輪
        int ROUND;

        // 調控小輪時間，次數越多，小輪時間越長
        float SCALE_TIME;

        // TODO: 新版偵測方式，是否不需要所謂的間隔時間?
        // 兩次動作之間的間隔時間
        float INTERVAL_TIME;

        // 兩輪之間的間隔時間
        float ROUND_INTERVAL_TIME;

        // 回饋呈現時間
        float FEEDBACK_DISPLAY_TIME;

        // 分數呈現時間
        float SCORE_DISPLAY_TIME;

        // ===== 音樂大小設定 =====
        // 正確回饋音量
        float CORRECT_VOL;

        // 成功回饋音量
        float SUCCESS_VOL;

        // 失敗回饋音量
        float FAIL_VOL;
        #endregion

        // 每一輪的時間
        float ROUND_TIME;

        // 目前玩了幾輪
        int round;

        // 每一輪要做幾次動作
        int NUMBER;

        // 目前還幾次動作要做
        int number;

        // 答對多少題
        int success;

        float round_time = 0f;

        // 用於平滑緩衝時間的不協調感
        float buffer_time;

        float detect_time = 0f;
        bool matched = false;

        // 呈現當前動作正確率與門檻值
        FloatList acc_list, thres_list, gap_list;

        private void Awake()
        {
            pm.init(n_player: 1);
            pm.getPlayer(0).setId("9527");
            gap_list = new FloatList(new float[] { 0 });
            thres_list = new FloatList(new float[] { 0 });
        }

        // Start is called before the first frame update
        void Start()
        {
            dm.initFileId();
            dm.initPlayer();

            questions = new List<string> {
                "Blue",
                "Red",
                "Yellow"
            };

            int i, rand, len = questions.Count;
            string temp;
            for (i = 0; i < 10; i++)
            {
                rand = Random.Range(1, len);
                temp = questions[0];
                questions[0] = questions[rand];
                questions[rand] = temp;
            }

            #region Game Config
            // 總共有幾輪
            ROUND = 3;

            // 調控小輪時間，次數越多，小輪時間越長
            SCALE_TIME = 5.0f;

            // 兩次動作之間的間隔時間
            INTERVAL_TIME = 0.8f;

            // 兩輪之間的間隔時間
            ROUND_INTERVAL_TIME = 0.25f;

            // 回饋呈現時間
            FEEDBACK_DISPLAY_TIME = 1.0f;

            // 分數呈現時間
            SCORE_DISPLAY_TIME = 3.0f;

            NUMBER = 5;

            // 每題 ROUND_TIME 秒(時間為題目長度的 SCALE_TIME 倍)
            ROUND_TIME = NUMBER * SCALE_TIME + 5f;

            #region 音樂大小設定
            // 正確回饋音量
            CORRECT_VOL = 30.0f;

            // 成功回饋音量
            SUCCESS_VOL = 70.0f;

            // 失敗回饋音量
            FAIL_VOL = 70.0f;
            #endregion
            #endregion

            // 目前玩了幾輪
            round = 0;

            // 目前還幾次動作要做
            number = NUMBER;

            // 計數更新
            ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));

            // 答對多少題
            success = 0;

            // 確保為關閉狀態
            success_image.SetActive(false);
            fail_image.SetActive(false);

            dm.addOnMatchedListener(onMatchedListener);

            // 全部配對成功
            dm.addOnAllMatchedFinishedListener(onAllMatchedFinishedListener);

            // 可以處理因超時而結束偵測的情況
            dm.addOnMatchEndedFinishedListener(onMatchEndedFinishedListener);

            dm.onAllResourcesLoaded.AddListener(()=> {
                StartCoroutine(gamePlaying());
            });

            StartCoroutine(gameStart());
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Application.Quit();
                }
            }
        }

        private void OnGUI()
        {
            GUI.skin.label.fontSize = 60;
            GUI.color = Color.black;

            gui = string.Format("\n\n\n\n\n\n\nROUND_TIME: {0}, pose: {1}\nround_time: {2:F4}\ndetect_time: {3:F4}\n" +
                "Gap(acc - thres): {4:F4}\nthreshold: {5:F4}", ROUND_TIME, pose, round_time, detect_time, 
                gap_list.sum(), thres_list.mean());

            GUILayout.Label(
                // text on gui
                gui,
                // start to define gui layout
                GUILayout.Width(Screen.width),
                GUILayout.Height(Screen.height));
        }

        private void OnDestroy()
        {

        }

        // ====================================================
        IEnumerator gameStart()
        {
            Debug.Log(string.Format("[IrtDemo2] gameStart"));

            #region regist 和 load 或許可以合併
            // 註冊標籤動作和實際動作之間的鏈結
            dm.registMultiPoses(Pose.RaiseTwoHands);
            dm.registMultiPoses(Pose.Squat);
            //dm.registMultiPoses(Pose.Hop, new List<Pose> {
            //    Pose.HopLeft, 
            //    Pose.HopRight
            //});
            dm.registMultiPoses(Pose.HopRight);
            yield return new WaitForSeconds(Time.deltaTime);

            // 載入各動作數據(會根據標籤動作取得內含的多動作)
            _ = dm.loadMultiPostures(Pose.RaiseTwoHands, Pose.Squat, Pose.HopRight);
            yield return new WaitForSeconds(Time.deltaTime); 
            #endregion

            //StartCoroutine(gamePlaying());
        }

        IEnumerator gamePlaying()
        {
            Debug.Log(string.Format("[IrtDemo2] gamePlaying"));
            
            string question;
            yield return new WaitForSeconds(Time.deltaTime);

            // 第幾輪
            while(round < ROUND)
            {
                round_time = 0f;

                // 目前還幾次動作要做
                number = NUMBER;

                // 出題 & 根據題目設定偵測的動作
                question = questions[round];
                question_image.sprite = Resources.Load<Sprite>(question);
                question_text.text = question;

                /* questions = new List<string> {
                 *  "Blue",
                 *  "Red",
                 *  "Yellow"
                 * };
                 */
                switch (question)
                {
                    case "Blue":
                        pose = Pose.Squat;
                        break;
                    case "Red":
                        pose = Pose.HopRight;
                        break;
                    case "Yellow":
                        pose = Pose.RaiseTwoHands;
                        break;
                    default:
                        pose = Pose.RaiseTwoHands;
                        break;
                }

                hint.sprite = Resources.Load<Sprite>(pose.ToString());
                Debug.Log(string.Format("[IrtDemo2] gamePlaying | question: {0}, pose: {1}", question, pose));

                foreach (Player player in pm.getPlayers())
                {
                    player.writeGameStage(GameStage.Game1);
                    player.setTargetPose(pose);
                }

                #region 一輪
                while (round_time < ROUND_TIME && 0 < number)
                {
                    Debug.Log(string.Format("[IrtDemo2] gamePlaying | round: {0}, number: {1}", round, number));

                    // 開始記錄骨架
                    dm.startRecord();

                    // detect function
                    dm.setDetectDelegate(defaultDetect);

                    // modify flag
                    //dm.setFlagDelegate(modifingFlag);

                    // 計數更新
                    ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));

                    while (round_time < ROUND_TIME && 0 < number && !matched)
                    {
                        round_time += Time.deltaTime;
                        yield return new WaitForSeconds(Time.deltaTime);
                    }

                    // invoke onMatchEnded
                    dm.stopRecord();

                    buffer_time = 0f;
                    while(buffer_time < INTERVAL_TIME)
                    {
                        buffer_time += Time.deltaTime;
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                }
                #endregion

                // 玩家完成指定次數(NUMBER)，剩餘次數(number)為 0
                if (number == 0)
                {
                    success++;
                    success_image.SetActive(true);
                    audio_manager.modifyVolumn(SUCCESS_VOL, 2);
                    audio_manager.play(AudioManager.AudioName.Success, 2);
                }
                else
                {
                    fail_image.SetActive(true);
                    audio_manager.modifyVolumn(FAIL_VOL, 2);
                    audio_manager.play(AudioManager.AudioName.Fail, 2);
                }

                // 清空提示圖案
                hint.sprite = null;

                // 呈現回饋 FEEDBACK_DISPLAY_TIME 秒後關閉
                buffer_time = 0f;
                while (buffer_time < FEEDBACK_DISPLAY_TIME)
                {
                    buffer_time += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }

                // 確保為關閉狀態
                success_image.SetActive(false);
                fail_image.SetActive(false);

                // 輪次計數加一
                round++;

                // Buffer time ROUND_INTERVAL_TIME
                buffer_time = 0f;
                while (buffer_time < ROUND_INTERVAL_TIME)
                {
                    buffer_time += Time.deltaTime;
                    yield return new WaitForSeconds(Time.deltaTime);
                }
            }

            audio_manager.fadeOut();
            StartCoroutine(gameEnd());
        }

        void defaultDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(pose);

            // 這個計時可用於決定是否需要觸發 onMatchEnded 事件
            detect_time += Time.deltaTime;

            // 透過 updateFlag 將 accumulate_time 傳給 modifingFlag，根據時間調整偵測模式
            //dm.updateFlag(detect_time);
            float delta_time;

            // 實作偵測，此形式保留了對個別動作的不同操作空間
            foreach (Pose pose in poses)
            {
                // 比對動作
                dm.compareMovement(player, pose);

                delta_time = Time.deltaTime;

                if ((detect_time < modify_milli) && (detect_time + delta_time > modify_milli))
                {
                    player.modifyThreshold(pose: pose);
                    Debug.Log(string.Format("[IrtDemo2] defaultDetect | modify_milli: {0}, detect_time: {1:F4}, round_time: {2:F4}",
                        modify_milli, detect_time, round_time));

                    // 間隔 interval_milli 毫秒再次呼叫調整門檻值的函式
                    modify_milli += interval_milli;
                }

                // 呈現當前動作正確率與門檻值
                acc_list = new FloatList(player.getAccuracy(pose));
                thres_list = new FloatList(player.getThreshold(pose));
                gap_list = acc_list - thres_list;
                int i, len = gap_list.length();
                for(i = 0; i < len; i++)
                {
                    if(gap_list[i] > 0.0f)
                    {
                        gap_list[i] = 0.0f;
                    }
                }
                Debug.Log(string.Format("[IrtDemo2] defaultDetect | pose: {0}\nacc: {1}\nthres: {2}", 
                    pose, acc_list.ToString(), thres_list.ToString()));
            }
        }

        Flag modifingFlag(float f)
        {
            // 配對(O), 修改門檻值(X)
            return Flag.Matching;
        }

        IEnumerator gameEnd()
        {
            Debug.Log(string.Format("[IrtDemo2] gameEnd"));

            // 關閉背景音樂
            audio_manager.stop();
            // 開始呈現分數, SCORE_DISPLAY_TIME 秒後結束呈現分數
            score_borad.setCorrect(success);
            score_borad.setWrong(ROUND - success);
            score_borad.display(SCORE_DISPLAY_TIME);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        void onMatchedListener(int index)
        {
            Debug.Log(string.Format("[IrtDemo2] onMatched(index: {0})", index));

            detect_time = 0f;
            modify_milli = 3.0f;
            matched = true;

            // 因此 number - 1
            number--;

            // 更新 UI 計數
            ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));            

            // 呈現當前動作正確率與門檻值
            Player player = pm.getPlayer(index);
            Pose matched_pose = player.getMatchedPose();
            Debug.Log(string.Format("[IrtDemo2] onMatched Listener: player {0} matched pose: {1}.", index, matched_pose));

            acc_list = new FloatList(player.getAccuracy(matched_pose));
            thres_list = new FloatList(player.getThreshold(matched_pose));
            gap_list = acc_list - thres_list;
            int i, len = gap_list.length();

            for (i = 0; i < len; i++)
            {
                if (gap_list[i] > 0.0f)
                {
                    gap_list[i] = 0.0f;
                }
            }

            // 正確音效
            audio_manager.modifyVolumn(CORRECT_VOL, 2);
            audio_manager.play(AudioManager.AudioName.Correct, 2);
        }

        void onAllMatchedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onAllMatchedFinishedListener"));
        }

        void onMatchEndedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onMatchEndedFinishedListener"));

            // 還原配對過程中的暫存資訊
            dm.resetState(pose);
            matched = false;
            detect_time = 0f;
            modify_milli = 3.0f;
        }
    }
}