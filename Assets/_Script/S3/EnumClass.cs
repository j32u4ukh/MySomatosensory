using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    public enum Pose
    {
        None,

        Hop,                // 單腳跳
        HopLeft,            // 左腳單腳跳
        HopRight,           // 右腳單腳跳

        Strike,             // 打擊
        StrikeLeft,         // 左打擊
        StrikeRight,        // 右打擊

        Kick,               // 踢
        KickLeft,           // 左踢
        KickRight,          // 右踢

        RaiseHand,          // 舉單手
        RaiseLeftHand,      // 舉雙手
        RaiseRightHand,     // 舉雙手

        Wave,               // 揮動(水平)
        WaveLeft,           // 左揮動(水平)
        WaveRight,          // 右揮動(水平)

        VerticalWave,       // 揮動(垂直)
        RaiseTwoHands,      // 舉雙手
        Jump,               // 雙腳跳
        Run,                // 跑
        CrossJump,          // 跨跳
        Stretch,            // 伸展
        Squat,              // 蹲下
        Dribble,            // 運球
        Walk,               // 走路
    }

    public enum GameStage { 
        Start,
        Game1,
        Game2,
        Game3,
        Test
    }
}