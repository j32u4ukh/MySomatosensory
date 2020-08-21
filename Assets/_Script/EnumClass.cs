using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace ETLab
{
    public enum Flag
    {
        None,       // 配對(X), 修改門檻值(X)
        Matching,   // 配對(O), 修改門檻值(X)
        Modify      // 配對(O), 修改門檻值(O)
    }

    // 包含實際動作 與 多動作的分類標籤(EX: Strike 包含 StrikeLeft 和 StrikeRight，但 Strike 本身不是實際動作)
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
        RaiseLeftHand,      // 舉左手
        RaiseRightHand,     // 舉右手

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
        Reverse,            // 扭轉
        Throw,              // 投擲

        // ========================
        JaNKeNPoN,          // 猜拳
    }

    public enum GameStage
    {
        Test = -1,
        Start = 0,
        Game1,
        Game2,
        Game3,
    }

    public enum AzureService
    {
        [Description("detect")]
        Detect,
        [Description("persongroups")]
        PersonGroupList,
        [Description("identify")]
        Identify,
        [Description("persongroups")]
        Person,

    }
}
