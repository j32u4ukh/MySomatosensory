using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S3
{
    // 用於場景間傳遞變數
    public static class GameInfo
    {
        //public static string id = "9527";

        public static string record_root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
