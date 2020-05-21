using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace S3
{
    public enum Position { 
        Left,
        Center,
        Right
    }

    public class MovingTest : MonoBehaviour
    {
        public GameObject player;
        public Button left;
        public Button right;

        Position pos = Position.Center;
        KinectManager kinectManager;
        Vector3 v3;

        // Start is called before the first frame update
        void Start()
        {
            v3 = transform.position;

            if (kinectManager == null)
            {
                kinectManager = KinectManager.Instance;
            }

            left.onClick.AddListener(() => {
                switch (pos)
                {
                    case Position.Left:
                        break;
                    case Position.Center:
                        pos = Position.Left;
                        break;
                    case Position.Right:
                        pos = Position.Center;
                        break;
                }

                move();
            });

            right.onClick.AddListener(() => {
                switch (pos)
                {
                    case Position.Left:
                        pos = Position.Center;
                        break;
                    case Position.Center:
                        pos = Position.Right;
                        break;
                    case Position.Right:
                        break;

                }

                move();
            });
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 trans = kinectManager.GetUserPosition(0);
            print(string.Format("trans: {0}", trans));

            //player.transform.localPosition = Vector3.zero;
            //player.transform.localRotation = Quaternion.identity;

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Q))
            {
                print("Application.Quit");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
		        Application.Quit();
#endif
            }
        }

        void move()
        {
            switch (pos)
            {
                case Position.Left:
                    transform.position = new Vector3(-1.0f, 0.0f, 0.0f);
                    break;
                case Position.Center:
                    transform.position = new Vector3(+0.0f, 0.0f, 0.0f);
                    break;
                case Position.Right:
                    transform.position = new Vector3(+1.0f, 0.0f, 0.0f);
                    break;
            }
        }
    }
}
