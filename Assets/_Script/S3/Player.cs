using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{    
    public class Player : MonoBehaviour
    {
        [HideInInspector] public int bones_number;

        protected AvatarController avatar_controller;
        protected PoseModelHelper model_helper;
        private string id;
        private int player_index;
        Transform right_hand;

        #region Data for remark
        string remark;
        #endregion

        private void Awake()
        {
            /* https://blog.csdn.net/u011185231/article/details/49523293
             * 問題: 其他腳本調用 Player 物件時產生NullReferenceException
             * 解決方法: 把最先實例化的全部放在Awake()方法中去
             */
            avatar_controller = GetComponent<AvatarController>();
            model_helper = GetComponent<PoseModelHelper>();
        }

        // Start is called before the first frame update
        void Start()
        {
            player_index = avatar_controller.playerIndex;
            right_hand = getBoneTransform(HumanBodyBones.RightHand);

            bones_number = getBonesNumber();
        }

        // Update is called once per frame
        void Update()
        {
            //print(string.Format("Player {0}: RightHand @ {1}", player_index, right_hand.position));
        }

        public int index()
        {
            return player_index;
        }

        #region PoseModelHelper
        public Transform getBoneTransform(int index)
        {
            return model_helper.GetBoneTransform(index);
        }

        public Transform getBoneTransform(HumanBodyBones bone)
        {
            int index = PoseModelHelper.boneToIndex(bone);

            if (index == -1)
            {
                return null;
            }

            return model_helper.GetBoneTransform(index);
        }

        public int getBonesNumber()
        {
            return model_helper.GetBoneTransformCount();
        }

        public HumanBodyBones indexToBones(int index)
        {
            return PoseModelHelper.indexToBone(index);
        }

        public int boneToIndex(HumanBodyBones bone)
        {
            return PoseModelHelper.boneToIndex(bone);
        }
        #endregion
    }
}
