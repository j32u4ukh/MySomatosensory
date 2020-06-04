using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    public static class ConfigData
    {
        // 一連續動作包含多少分解動作
        public static int n_posture = 30;

        public static float min_threshold = 0.5f;
        public static float max_threshold = 1.0f;
        public static float init_threshold = (min_threshold + max_threshold);
        public static float learning_rate = 0.05f;
    }
}
