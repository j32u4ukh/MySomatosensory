using System.Collections;
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
        float boundary_time = 5f;

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

        // 音樂大小設定
        // 背景音量
        float BG_VOL;

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

        float round_time;
        float detect_time = 0f;
        bool matched = false;

        private void Awake()
        {
            pm.init(n_player: 1);
            pm.getPlayer(0).setId("9527");

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
            SCALE_TIME = 3.0f;

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
            // 背景音量
            BG_VOL = 60.0f;

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

            StartCoroutine(gameStart());
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q))
            {
                Application.Quit();
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 60;
            gui = string.Format("ROUND_TIME: {0:F4}, round_time: {1:F4}\ndetect_time: {2:F4}", ROUND_TIME, round_time, detect_time);

            GUILayout.Label(
                // text on gui
                gui,
                // start to define gui layout
                GUILayout.Width(Screen.width / 2f),
                GUILayout.Height(Screen.height / 2f));
        }

        private void OnDestroy()
        {

        }

        // ====================================================
        IEnumerator gameStart()
        {
            Debug.Log(string.Format("[IrtDemo2] gameStart"));

            // 註冊標籤動作和實際動作之間的鏈結
            dm.registMultiPoses(Pose.RaiseTwoHands);
            dm.registMultiPoses(Pose.Squat);
            dm.registMultiPoses(Pose.Hop, new List<Pose> {
                Pose.HopLeft, 
                Pose.HopRight
            });
            yield return new WaitForSecondsRealtime(Time.deltaTime);

            // 載入各動作數據(會根據標籤動作取得內含的多動作)
            dm.loadMultiPosture(Pose.RaiseTwoHands);
            dm.loadMultiPosture(Pose.Squat);
            dm.loadMultiPosture(Pose.Hop);
            yield return new WaitForSecondsRealtime(Time.deltaTime);

            StartCoroutine(gamePlaying());
        }

        IEnumerator gamePlaying()
        {
            Debug.Log(string.Format("[IrtDemo2] gamePlaying"));
            
            string question;
            yield return new WaitForSecondsRealtime(Time.deltaTime);

            // 第幾輪
            while(round < ROUND)
            {
                round_time = 0f;

                // 目前還幾次動作要做
                number = NUMBER;

                // TODO: 出題 & 根據題目設定偵測的動作
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

                pose = Pose.Squat;
                question = "Blue";
                Debug.Log(string.Format("[IrtDemo2] gamePlaying | question: {0}, pose: {1}", question, pose));

                foreach (Player player in pm.getPlayers())
                {
                    player.setGameStage(GameStage.Game1);
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
                    dm.setFlagDelegate(modifingFlag);

                    // 計數更新
                    ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));

                    round_time += Time.deltaTime;
                    yield return new WaitForSecondsRealtime(Time.deltaTime);

                    while (round_time < ROUND_TIME && 0 < number && !matched)
                    {
                        round_time += Time.deltaTime;
                        gui = string.Format("ROUND_TIME: {0:F4}, round_time: {1:F4}", ROUND_TIME, round_time);
                        yield return new WaitForSecondsRealtime(Time.deltaTime);
                    }

                    // invoke onMatchEnded
                    dm.stopRecord();
                    yield return new WaitForSeconds(INTERVAL_TIME);
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

                // 呈現回饋 FEEDBACK_DISPLAY_TIME 秒後關閉
                yield return new WaitForSeconds(FEEDBACK_DISPLAY_TIME);

                // 確保為關閉狀態
                success_image.SetActive(false);
                fail_image.SetActive(false);

                // 輪次計數加一
                round++;

                // Buffer time ROUND_INTERVAL_TIME
                yield return new WaitForSeconds(ROUND_INTERVAL_TIME);
            }

            audio_manager.fadeOut();
            StartCoroutine(gameEnd());
        }

        void defaultDetect(Player player)
        {
            List<Pose> poses = dm.getPoses(pose);

            // TODO: 這個計時可用於決定是否需要觸發 onMatchEnded 事件
            detect_time += Time.deltaTime;

            // 透過 updateFlag 將 accumulate_time 傳給 modifingFlag，根據時間調整偵測模式
            dm.updateFlag(detect_time);

            // 實作偵測，此形式保留了對個別動作的不同操作空間
            foreach (Pose pose in poses)
            {
                // 比對動作
                dm.compareMovement(player, pose);
            }
        }

        Flag modifingFlag(float f)
        {
            if (detect_time > boundary_time)
            {
                // 配對(O), 修改門檻值(O)
                return Flag.Modify;
            }
            else
            {
                // 配對(O), 修改門檻值(X)
                return Flag.Matching;
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
            yield return new WaitForSecondsRealtime(Time.deltaTime);
        }

        void onMatchedListener(int index)
        {
            Debug.Log(string.Format("[IrtDemo2] onMatched(index: {0})", index));

            detect_time = 0f;
            matched = true;

            // 因此 number - 1
            number--;

            // 更新 UI 計數
            ui_count.sprite = Resources.Load<Sprite>(string.Format("number{0}", number));

            // 正確音效
            audio_manager.modifyVolumn(CORRECT_VOL, 2);
            audio_manager.play(AudioManager.AudioName.Correct, 2);

            // Debug.Log(string.Format("[IrtDemo2] onMatched Listener: player {0} matched.", index));
            Pose pose = pm.getPlayer(index).getMatchedPose();
            Debug.Log(string.Format("[IrtDemo2] onMatched Listener: player {0} matched pose: {1}.", index, pose));
        }

        void onAllMatchedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onAllMatchedFinishedListener"));
        }

        void onMatchEndedFinishedListener()
        {
            Debug.Log(string.Format("[IrtDemo2] onMatchEndedFinishedListener"));

            //Player player = pm.getPlayer(0);
            //Movement movement = player.getMovement(pose);
            //float[] acc = movement.getAccuracy();
            //Debug.Log(string.Format("[IrtDemo2] Accuracy: {0}", Utils.arrayToString(acc)));
            //Debug.Log(string.Format("[IrtDemo2] Threshold: {0}", Utils.arrayToString(movement.getThresholds())));

            // 還原配對過程中的暫存資訊
            dm.resetState(pose);
            matched = false;
        }
    }
}