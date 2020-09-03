using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace ETLab
{
    public static class Utils
    {
        // 動作名稱 與 動作物件(不同於 enum Pose，這裡僅包含實際動作，不包含多動作的分類標籤)
        public static Pose[] poses = {
            Pose.HopLeft,            // 左腳單腳跳
            Pose.HopRight,           // 右腳單腳跳

            Pose.StrikeLeft,         // 左打擊
            Pose.StrikeRight,        // 右打擊

            Pose.KickLeft,           // 左踢
            Pose.KickRight,          // 右踢

            Pose.RaiseLeftHand,      // 舉左手
            Pose.RaiseRightHand,     // 舉右手

            Pose.WaveLeft,           // 左揮動(水平)
            Pose.WaveRight,          // 右揮動(水平)

            Pose.VerticalWave,       // 揮動(垂直)
            Pose.RaiseTwoHands,      // 舉雙手
            Pose.Jump,               // 雙腳跳
            Pose.Run,                // 跑
            Pose.CrossJump,          // 跨跳
            Pose.Stretch,            // 伸展
            Pose.Squat,              // 蹲下
            Pose.Dribble,            // 運球
            Pose.Walk                // 走路
        };

        // 產生抽樣的索引值
        public static List<int> sampleIndex(int length, int n_sample = 1)
        {
            List<int> indexs = new List<int>();
            int start, end, rand, i;

            if (n_sample > length)
            {
                n_sample = length;
            }

            // 每一個區塊的大小(將 length=100 區分為 n_sample=5 區，每一區的大小為 sample_size=20)
            float sample_size = (float)length / n_sample;
            System.Random random = new System.Random();
            for (i = 0; i < n_sample; i++)
            {
                start = (int)(sample_size * i);
                end = (int)(start + sample_size);
                rand = random.Next(start, end);
                indexs.Add(rand);
            }

            return indexs;
        }

        // 根據樣本數抽樣
        public static List<T> sampleList<T>(List<T> list, int n_sample = 1)
        {
            List<T> sample = new List<T>();
            List<int> indexs = sampleIndex(list.Count, n_sample);

            foreach (var index in indexs)
            {
                sample.Add(list[index]);
            }

            return sample;
        }

        // 修改檔名後綴
        public static void renameSuffix(string path, string suffix)
        {
            FileInfo info = new FileInfo(path);
            string file_name = info.Name.Split('.')[0];

            // 檢查是否已經有後綴名稱
            if (file_name.Split('_').Length == 1)
            {
                // 後綴已為欲修改內容，跳出
                if (file_name.Split('_')[1].Equals(suffix))
                {
                    return;
                }
                // 後綴為其他內容，修改後綴名稱
                else
                {
                    string new_name = string.Format("{0}_{1}", file_name.Split('_')[0], suffix);
                    rename(path, new_name);
                }
            }
            else
            {
                string new_name = string.Format("{0}_{1}", file_name, suffix);
                rename(path, new_name);
            }
        }

        // 修改檔名
        public static void rename(string path, string new_name)
        {
            FileInfo info = new FileInfo(path);
            string dir = info.DirectoryName;
            string file_type = info.Name.Split('.')[1];
            string new_path = Path.Combine(dir, string.Format("{0}.{1}", new_name, file_type));
            File.Move(path, new_path);
        }

        // 返回格式化陣列的字串
        public static string arrayToString<T>(T[] array)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            int i, len = array.Length;

            if(len > 0)
            {
                for (i = 0; i < len - 1; i++)
                {
                    sb.Append(string.Format("{0:F4}, ", array[i].ToString()));
                }

                sb.Append(string.Format("{0:F4}", array[len - 1].ToString()));
            }

            sb.Append("]");

            return sb.ToString();
        }

        public static string listToString<T>(List<T> list)
        {
            return arrayToString(list.ToArray());
        }

        /// <summary>
        /// 取得能力估計函數的值
        /// </summary>
        /// <param name="theta">玩家的能力</param>
        /// <param name="beta">門檻值</param>
        /// <returns></returns>
        public static float getIrtValue(float theta, float beta)
        {
            float e = Mathf.Pow((float)Math.E, theta - beta);

            return e / (1f + e);
        }

        #region alpha: 能力估計函數 的 X 軸數值
        // 將 0 ~ 1 的機率映射到 -3 ~ +3
        public static float pToAlpha(float p)
        {
            return (p * 6f) - 3f;
        }

        // 將 -3 ~ +3 的機率映射到 0 ~ 1
        public static float alphaToP(float alpha)
        {
            return (alpha + 3f) / 6f;
        }
        #endregion

        // ============================================================
        
        public static string getDescription(string value, Type type)
        {
            string name = Enum.GetNames(type).Where(f => f.Equals(value, StringComparison.CurrentCultureIgnoreCase))
                                             .Select(d => d)
                                             .FirstOrDefault();

            //// 找無相對應的列舉
            if (name == null)
            {
                return string.Empty;
            }

            // 利用反射找出相對應的欄位
            var field = type.GetField(name);

            // 取得欄位設定DescriptionAttribute的值
            var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //// 無設定Description Attribute, 回傳Enum欄位名稱
            if (attribute == null || attribute.Length == 0)
            {
                return name;
            }

            //// 回傳Description Attribute的設定
            return ((DescriptionAttribute)attribute[0]).Description;
        }

        #region 無法藉由點擊 Editor log 跳到實際腳本位置
        /* 以下屬性只能置於參數位置，不能於函式內呼叫
         * CallerLineNumber: 實際呼叫的行數位置
         * CallerMemberName: 實際呼叫的函數名稱
         * CallerFilePath: 實際呼叫的腳本路徑
         * 參考網站: https://stackoverflow.com/questions/12556767/how-do-i-get-the-current-line-number
         */
        public static void log(string message, [CallerLineNumber] int line_num = 0, [CallerMemberName] string member = "", [CallerFilePath] string file_path = "")
        {
            message = string.Format("[{0}] ({1}) {2}\n{3}", member, line_num, message, file_path);
            Debug.Log(message);
        }

        public static void error(string message, [CallerLineNumber] int line_num = 0, [CallerMemberName] string member = "", [CallerFilePath] string file_path = "")
        {
            message = string.Format("[{0}] ({1}) {2}\n{3}", member, line_num, message, file_path);
            Debug.LogError(message);
        }
        #endregion
    }
}
