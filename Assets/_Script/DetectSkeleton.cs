using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DetectSkeleton
{
    None,
    //遊戲場景動作
    DoubleFootJump,         // 雙腳跳
    Running,                // 跑

    SingleFootJump,         // 單腳跳
    SingleLeftFootJump,     // 左腳單腳跳
    SingleRightFootJump,    // 右腳單腳跳

    CrossJump,              // 跨跳
    Stretch,                // 伸展
    Squat,                  // 蹲下

    WaveHit,                // 揮動
    WaveLeftHit,            // 左揮動
    WaveRightHit,           // 右揮動

    Reverse,                // 扭轉
    ReverseLeft,            // 左扭轉
    ReverseRight,           // 右扭轉

    Strike,                 // 打擊
    StrikeLeft,             // 左打擊
    StrikeRight,            // 右打擊

    Kick,                   // 踢
    KickLeft,               // 左踢
    KickRight,              // 右踢

    Dribble,                // 運球

    //遊戲進程動作
    PutHandsUp,             // 半舉雙手
    Walking,                // 走路
}