using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public class DetectManager : MonoBehaviour
    {
        /* TODO:
         * 姿勢匹配(bool[] is_matched) 與 額外條件(bool is_additional_matched) 都要通過才是真的通過
         */
        public Pose pose;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /* TODO: float getAccuracy(Player player, List<HumanBodyBones> comparingParts){
         *     modelHelper.GetBoneTransform(index) >>> player.getBoneTransform(index)
         *     poseModelHelper.GetBoneTransform(index) >>> 
         * } 
         */
    }
}
