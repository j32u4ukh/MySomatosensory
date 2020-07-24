using ETLab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ETLab
{
    public class Game1SceneTest : MonoBehaviour
    {
        PlayerManager pm;
        DetectManager dm;

        // Start is called before the first frame update
        void Start()
        {
            pm = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
            dm = GameObject.Find("DetectManager").GetComponent<DetectManager>();

            foreach(Player player in pm.getPlayers())
            {
                Debug.Log(string.Format("[Game1SceneTest] Start | player {0}, id: {1}", 
                    player.index(), player.getId()));
            }

            //dm.onMatched.AddListener((int index) => {
            //    Debug.Log(string.Format("[Game1SceneTest] onMatched Listener: player {0} matched.", index));
            //});
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(string.Format("[StartSceneTest] Update | loadMultiPosture"));
                dm.loadMultiPosture(Pose.RaiseTwoHands);
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                dm.setDetectDelegate(detectRaiseTwoHands);
            }

            // 跳場景
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    SceneManager.LoadScene(string.Format("_Scenes/StartScene"));
                }
            }
        }

        void detectRaiseTwoHands(Player player)
        {
            List<Pose> poses = dm.getPoses(Pose.RaiseTwoHands);

            foreach (Pose pose in poses)
            {
                dm.compareMovement(player, pose);
            }
        }
    }
}