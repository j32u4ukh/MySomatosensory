using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public class DetectManagerTest : MonoBehaviour
    {
        public DetectManager dm;

        // Start is called before the first frame update
        void Start()
        {
            // 可取得個別玩家通過時的資訊，在此處理所有玩家皆完成、並非全部玩家皆完成時的情況
            dm.onMatched.AddListener((int index)=> {
                Debug.Log(string.Format("[DetectManagerTest] onMatched Listener player {0} matched.", index));
            });


        }

        // Update is called once per frame
        void Update()
        {

        }

        void detectRaiseTwoHands()
        {

        }
    }
}
