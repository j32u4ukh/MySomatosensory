using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public class PlayerTest : MonoBehaviour
    {
        public Player player;

        // Start is called before the first frame update
        void Start()
        {
            
            int num = player.getBonesNumber();
            print(string.Format("getBonesNumber: {0}", num));
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
