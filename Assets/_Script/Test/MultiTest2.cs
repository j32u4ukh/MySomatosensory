using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace ETLab
{
    public class MultiTest2 : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Azure.initConfigData();
            //_ = getPersonGroupList();
            // G5-4: "20200821 face id": 93dec0ee-4bcb-43e0-847a-eb66bbec5892  "personId": "bd5acccd-990c-4915-8685-1b80d984960f"
            //_ = getPersonTask(group_id: "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7", person_id: "bd5acccd-990c-4915-8685-1b80d984960f");
            //_ = postFaceDetect(file_name: "G5-4.PNG");
            _ = postFaceIdentify(group_id: "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7", file_name: "G12345-5.PNG");
            //_ = postFaceIdentify2(file_name: "detection1.jpg");

            // detection1: "20200821 face id": 8c4db702-99f1-4e7c-9b58-76e7a7892733
            //_ = postFaceDetect(file_name: "detection1.jpg");
        }

        private void FixedUpdate()
        {

        }


        // Update is called once per frame
        void Update()
        {

        }

        private void OnDestroy()
        {

        }

        async Task postFaceIdentify(string group_id, string file_name)
        {
            List<Person> people = await Azure.postFaceIdentify(group_id, file_name);

            foreach(Person person in people)
            {
                Debug.Log(person);
            }
        }

        /// <summary>
        /// 偵測並返回圖片中的人臉及位置
        /// </summary>
        /// <param name="file_name">要偵測的檔案的名稱(含副檔名)</param>
        /// <returns></returns>
        async Task postFaceDetect(string file_name)
        {
            FaceDetects face_detects = await Azure.postFaceDetect(file_name);
            Debug.Log(face_detects);
        }

        /// <summary>
        /// getPersonGroupList 測試
        /// </summary>
        /// <returns></returns>
        async Task getPersonGroupList()
        {
            PersonGroupList list = await Azure.getPersonGroupList();
            Debug.Log(list);
        }

        string jsonParser(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return string.Empty;
            }


            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore)
                        {
                            quote = !quote;
                        }
                        break;
                    case '\'':
                        if (quote)
                        {
                            ignore = !ignore;
                        }
                        break;
                }

                if (quote)
                {
                    sb.Append(ch);
                }
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
