using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ETLab
{
    

    public class FaceDetects
    {
        /*
        [
            {
                "faceId": "54a0cda7-6786-4809-ada2-3deb337a69f5",
                "faceRectangle": 
                {
                    "top": 437,
                    "left": 121,
                    "width": 226,
                    "height": 226
                }
            }, 
            {
                "faceId": "83bc8fe5-160a-43fa-964b-e1751d8acbe7",
                "faceRectangle": 
                {
                    "top": 307,
                    "left": 340,
                    "width": 189,
                    "height": 189
                }
            }
        ]
        */
        public List<FaceDetect> detects;

        public List<string> getFaceIds()
        {
            List<string> ids = new List<string>();

            for (int i = 0; i < detects.Count; i++)
            {
                ids.Add(detects[i].faceId);
            }

            return ids;
        }

        public int getDetectedNumber()
        {
            return detects.Count;
        }

        public override string ToString()
        {
            int n_detect = detects.Count;

            if (n_detect == 0)
            {
                return "圖片中未偵測到人臉";
            }
            else if (n_detect == 1)
            {
                return string.Format("偵測到1張人臉 \n[({0})]", detects[0].ToString());
            }
            else
            {
                StringBuilder sb = new StringBuilder(string.Format("偵測到 {0} 張人臉 \n[", n_detect));

                for (int i = 0; i < n_detect; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(",\n");
                    }

                    sb.Append(string.Format("({0})", detects[i].ToString()));
                }

                sb.Append("]");

                return sb.ToString();
            }
        }

        /// <summary>
        /// 原始輸入的 Json 數據沒有 detects 標籤，但這是套件轉換時所必須的，故自行添加
        /// </summary>
        /// <param name="json_data"></param>
        /// <returns></returns>
        public static FaceDetects loadData(string json_data)
        {
            json_data = string.Format("{{'detects': {0}}}", json_data);
            return JsonConvert.DeserializeObject<FaceDetects>(json_data);
        }
    }

    public class FaceDetect
    {
        public string faceId;
        public Dictionary<string, int> faceRectangle;
        public override string ToString()
        {
            return string.Format("face_id: {0}, top: {1}, left: {2}, width: {1}, height: {2}",
                faceId, faceRectangle["top"], faceRectangle["left"], faceRectangle["width"], faceRectangle["height"]);
        }

        public static FaceDetect loadData(string json_data)
        {
            return JsonConvert.DeserializeObject<FaceDetect>(json_data);
        }
    }
}
