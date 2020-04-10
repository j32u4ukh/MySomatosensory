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
        private int player_index;
        Transform right_hand;

        #region Data for remark
        string remark; 
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            avatar_controller = GetComponent<AvatarController>();
            model_helper = GetComponent<PoseModelHelper>();

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

        public Transform getBoneTransform(HumanBodyBones bones)
        {
            int index = PoseModelHelper.boneToIndex(bones);
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
        #endregion
    }
}
