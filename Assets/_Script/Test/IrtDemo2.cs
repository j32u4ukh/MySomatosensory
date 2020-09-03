using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private Pose training_pose;

        string gui = "";
        float modify_buffer = 3.0f;
        float interval_buffer = 0.1f;
        bool logged_in = false;
        bool is_training = true;

        // ====================================================================================================
        // 音效管理
        public AudioManager audio_manager;

        List<string> questions;

        #region UI
        public GameObject login_buffer;
        public Button login_button;

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

        // 當前 demo 為單人遊戲，故，在一位玩家通過後 matched 就改變狀態，結束當下的迴圈
        bool matched = false;

        // 呈現當前動作正確率與門檻值
        FloatList acc_list, thres_list, gap_list;

        private void Awake()
        {
            Azure.initConfigData(config_path: @"D:\Unity Projects\AzureFaceConfig.txt");

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

            // 練習一動作完成
            dm.addOnMatchedListener(onTrainingMatchedListener);

            // 全部配對成功
            dm.addOnAllMatchedFinishedListener(onTrainingAllMatchedFinishedListener);

            // 可以處理因超時而結束偵測的情況
            dm.addOnMatchEndedFinishedListener(onTrainingMatchEndedFinishedListener);

            dm.onAllResourcesLoaded.AddListener(()=> {
                //StartCoroutine(gamePlaying());
                StartCoroutine(poseTraining());

            });

            login_button.onClick.AddListener(()=> {
                //_ = identifyPlayer(group_id: "noute_and_miyu_group");
                login_buffer.SetActive(false);
                logged_in = true;
                StartCoroutine(gameStart());
            });
        }

        // Update is called once per frame
        void Update()
        {
            // 離開遊戲
            if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Application.Quit();
                }
            }

            // 感測器辨識測試
            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("開始: 感測器辨識測試");
                _ = postFaceDetect("orbbec");
                Debug.Log("結束: 感測器辨識測試");
            }
        }

        async Task identifyPlayer(string group_id)
        {
            List<Person> people = await Azure.postFaceIdentify(group_id, "orbbec");

            foreach (Person person in people)
            {
                if(person.name.Equals("noute")  || person.name.Equals("miyu"))
                {
                    Debug.Log(string.Format("{0} login~", person.name));
                    login_buffer.SetActive(false);
                    logged_in = true;
                    StartCoroutine(gameStart());
                    break;
                }
            }
        }

        /// <summary>
        /// 偵測並返回圖片中的人臉及位置
        /// </summary>
        /// <param name="file_name">要偵測的檔案的名稱(含副檔名)</param>
        /// <returns></returns>
        async Task postFaceDetect(string file_name)
        {
            FaceDetects face_detects = await Azure.postFaceDetect(file_name);
            Debug.Log(face_detects);
        }

        private void OnGUI()
        {
            if (logged_in)
            {
                GUI.skin.label.fontSize = 60;
                GUI.color = Color.black;

                if (is_training)
                {
                    gui = string.Format("\n\n\n\n\n\n\nPose: {0}, \ndetect_time: {1:F4}\nGap(acc - thres): {2:F4}\nthreshold: {3:F4}", 
                        training_pose, detect_time, gap_list.sum(), thres_list.mean());

                }
                else
                {
                    gui = string.Format("\n\n\n\n\n\n\nROUND_TIME: {0}, pose: {1}\nround_time: {2:F4}\ndetect_time: {3:F4}\n" +
                        "Gap(acc - thres): {4:F4}\nthreshold: {5:F4}", ROUND_TIME, pose, round_time, detect_time,
                        gap_list.sum(), thres_list.mean());

                }

                GUILayout.Label(
                    // text on gui
                    gui,
                    // start to define gui layout
                    GUILayout.Width(Screen.width),
                    GUILayout.Height(Screen.height));
            }
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
            dm.registMultiPoses(Pose.Hop, new List<Pose> {
                Pose.HopLeft,
                Pose.HopRight
            });
            //dm.registMultiPoses(Pose.HopRight);
            yield return new WaitForSeconds(Time.deltaTime);

            // 載入各動作數據(會根據標籤動作取得內含的多動作)
            _ = dm.loadMultiPostures(Pose.RaiseTwoHands, Pose.Squat, Pose.Hop);
            //_ = dm.loadMultiPostures(Pose.RaiseTwoHands, Pose.Squat, Pose.HopRight);
            yield return new WaitForSeconds(Time.deltaTime);
            #endregion

            // loadComparingParts 和 loadMultiPostures 都載入完成後，會觸發 onAllResourcesLoaded，接著遊戲開始
        }

        IEnumerator gamePlaying()
        {
            dm.detect_mode = DetectMode.Testing;
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
                        pose = Pose.Hop;
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

                    // 還原配對過程中的暫存資訊: 至於每次開始偵測前，為確保偵測結束後，調整門檻時，有正確率等數據可以使用
                    dm.resetState(pose);

                    // 開始記錄骨架
                    dm.startRecord();

                    // detect function
                    dm.setDetectDelegate(defaultDetect);

                    // 計數更新
                    ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));

                    while (round_time < ROUND_TIME && 0 < number && !matched)
                    {
                        round_time += Time.deltaTime;
                        yield return new WaitForSeconds(Time.deltaTime);
                    }

                    // stopRecord will invoke onMatchEnded
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

            float delta_time;

            // 實作偵測，此形式保留了對個別動作的不同操作空間
            foreach (Pose pose in poses)
            {
                // 比對動作
                dm.compareMovement(player, pose);

                delta_time = Time.deltaTime;

                if ((detect_time < modify_buffer) && (detect_time + delta_time > modify_buffer))
                {
                    // TODO: 遊戲模式中，應該不能調整門檻值
                    player.modifyThreshold(pose: pose);
                    Debug.Log(string.Format("[IrtDemo2] defaultDetect | modify_buffer: {0}, detect_time: {1:F4}, round_time: {2:F4}",
                        modify_buffer, detect_time, round_time));

                    // 間隔 interval_milli 毫秒再次呼叫調整門檻值的函式
                    modify_buffer += interval_buffer;
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

        #region 練習動作用
        IEnumerator poseTraining()
        {
            // TODO: 目前為練習一次，之後或許可以改成持續練習動作 N 次或是直到不需要調整門檻值
            dm.detect_mode = DetectMode.Training;

            foreach (Pose t_pose in new Pose[] { Pose.RaiseTwoHands, Pose.Squat, Pose.HopRight, Pose.HopLeft })
            {
                Utils.log(string.Format("start training {0}", t_pose));
                detect_time = 0f;
                interval_buffer = 0.1f;
                training_pose = t_pose;
                matched = false;

                foreach (Player player in pm.getPlayers())
                {
                    player.writeGameStage(GameStage.Test);
                    player.setTargetPose(training_pose);
                }

                // 還原配對過程中的暫存資訊: 至於每次開始偵測前，為確保偵測結束後，調整門檻時，有正確率等數據可以使用
                dm.resetState(training_pose);
                yield return new WaitForSeconds(2f);

                // 開始記錄骨架
                dm.startRecord();

                // detect function
                dm.setDetectDelegate(trainingDetect);

                while (!matched)
                {
                    Utils.log(string.Format("training is matched? {0}", matched));
                    yield return new WaitForSeconds(Time.deltaTime);
                }

                // stopRecord will invoke onMatchEnded
                dm.stopRecord();
                Utils.log(string.Format("call stopRecord, matched: {0}", matched));
                
            }

            login_buffer.SetActive(true);
            yield return new WaitForSeconds(2f);
            login_buffer.SetActive(false);

            dm.resetListener();

            dm.addOnMatchedListener(onMatchedListener);

            // 全部配對成功
            dm.addOnAllMatchedFinishedListener(onAllMatchedFinishedListener);

            // 可以處理因超時而結束偵測的情況
            dm.addOnMatchEndedFinishedListener(onMatchEndedFinishedListener);

            StartCoroutine(gamePlaying());
        }

        void trainingDetect(Player player)
        {
            // 這個計時可用於決定是否需要觸發 onMatchEnded 事件
            detect_time += Time.deltaTime;

            // 比對動作
            dm.compareMovement(player, training_pose);

            if ((detect_time < modify_buffer) && (detect_time + Time.deltaTime > modify_buffer))
            {
                // TODO: 練習動作時，最一開始的 嘗試時間 以及 更新間隔時間 都會越來越長
                player.modifyThreshold(pose: training_pose);
                Debug.Log(string.Format("[IrtDemo2] defaultDetect | modify_milli: {0}, detect_time: {1:F4}, round_time: {2:F4}",
                    modify_buffer, detect_time, round_time));

                // 間隔 interval_milli 毫秒再次呼叫調整門檻值的函式
                modify_buffer += interval_buffer;
                interval_buffer += 0.1f;
            }

            // 呈現當前動作正確率與門檻值
            acc_list = new FloatList(player.getAccuracy(training_pose));
            thres_list = new FloatList(player.getThreshold(training_pose));
            gap_list = acc_list - thres_list;
            int i, len = gap_list.length();
            for (i = 0; i < len; i++)
            {
                if (gap_list[i] > 0.0f)
                {
                    gap_list[i] = 0.0f;
                }
            }
            Debug.Log(string.Format("[IrtDemo2] trainingDetect | pose: {0}\nacc: {1}\nthres: {2}",
                training_pose, acc_list.ToString(), thres_list.ToString()));
        }

        void onTrainingMatchedListener(int index)
        {
            Debug.Log(string.Format("[IrtDemo2] onTrainingMatchedListener(index: {0})", index));

            detect_time = 0f;
            modify_buffer = 3.0f;
            matched = true;

            // 呈現當前動作正確率與門檻值
            Player player = pm.getPlayer(index);
            Pose matched_pose = player.getMatchedPose();
            Debug.Log(string.Format("[IrtDemo2] onTrainingMatchedListener Listener: player {0} matched pose: {1}.", index, matched_pose));

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

        void onTrainingAllMatchedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onTrainingAllMatchedFinishedListener"));
        }

        void onTrainingMatchEndedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onTrainingMatchEndedFinishedListener"));

            matched = false;
            detect_time = 0f;
            modify_buffer = 3.0f;
        } 
        #endregion

        #region 遊戲中正式配對
        void onMatchedListener(int index)
        {
            Debug.Log(string.Format("[IrtDemo2] onMatched(index: {0})", index));

            detect_time = 0f;
            modify_buffer = 3.0f;
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

            matched = false;
            detect_time = 0f;
            modify_buffer = 3.0f;
        } 
        #endregion
    }
}