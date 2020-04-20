using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace S3
{
    public static class Utils
    {
        // 返回格式化陣列的字串
        public static string arrayToString<T>(T[] array)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");

            int i, len = array.Length;
            for(i = 0; i < len - 1; i++)
            {
                sb.Append(string.Format("{0:F2}, ", array[i]));
            }

            sb.Append(string.Format("{0:F2}]", array[len - 1]));

            return sb.ToString();
        }

        // 產生抽樣的索引值
        public static List<int> sampleIndex(int length, int n_sample = 1)
        {
            List<int> indexs = new List<int>();
            int start, end, rand, i;

            if (n_sample > length)
            {
                n_sample = length;
            }

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

        // 根據索引值抽樣
        public static List<T> sampleList<T>(List<T> list, List<int> indexs)
        {
            List<T> sample = new List<T>();

            foreach (var index in indexs)
            {
                sample.Add(list[index]);
            }

            return sample;
        }

        // 修改檔名
        public static void rename(string path, string new_name){
            FileInfo info = new FileInfo(path);
            string dir = info.DirectoryName;
            string file_type = info.Name.Split('.')[1];
            string new_path = Path.Combine(dir, string.Format("{0}.{1}", new_name, file_type));
            File.Move(path, new_path);
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

    }
}
