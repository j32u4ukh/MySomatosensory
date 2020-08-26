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
            string group_id = "noute_and_miyu_group";
            string group_name = "noute_and_miyu";
            string person_id = "79492b1f-47e9-4b2a-8a0b-4398c9be8dab";
            string file_name = "miyu (7).jpg";

            // G5-4: "20200821 face id": 93dec0ee-4bcb-43e0-847a-eb66bbec5892  "personId": "bd5acccd-990c-4915-8685-1b80d984960f"
            //_ = getPersonTask(group_id: "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7", person_id: "bd5acccd-990c-4915-8685-1b80d984960f");
            //_ = postFaceDetect(file_name: file_name);
            //_ = postFaceIdentify(group_id: "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7", file_name: "B345-5.PNG");
            _ = postFaceIdentify(group_id: group_id, file_name: file_name);
            //_ = postFaceIdentify2(file_name: "detection1.jpg");
            // detection1: "20200821 face id": 8c4db702-99f1-4e7c-9b58-76e7a7892733
            //_ = postFaceDetect(file_name: "detection1.jpg");

            //_ = getPersonGroupList();
            //_ = createPersonGroup(group_id: group_id, group_name: group_name);
            //_ = putPerseonGroupCreate(group_id, group_name);
            //_ = deletePersonGroupDelete(group_id);

            //_ = postPersonCreate(group_id, person_name: "miyu8");
            //_ = getPersonList(group_id);
            //_ = postPersonAddFace(group_id, person_id, file_name: "miyu (7).jpg");
            //_ = postPersonGroupTrain(group_id);

            //string group_id = "group_id_is_miyu";
            //_ = deletePersonDelete(group_id, person_id: "4c2f25f3-3191-4430-bdcf-65c56cd97535");
        }

        async Task createPersonGroup(string group_id, string group_name)
        {
            Dictionary<string, List<string>> people = new Dictionary<string, List<string>> {
                { "miyu", new List<string>{ "miyu (1).jpg", "miyu (2).jpg", "miyu (3).jpg", "miyu (4).jpg", "miyu (5).jpg" } },
                { "noute", new List<string>{ "noute1.jpg", "noute2.jpg", "noute3.jpg", "noute4.jpg", "noute5.jpg" } }
            };

            await Azure.createPersonGroup(group_id: group_id, group_name: group_name, people: people);
        }

        async Task putPerseonGroupCreate(string group_id, string group_name)
        {           
            //Dictionary<string, List<string>> people = new Dictionary<string, List<string>> {
            //    { "miyu", new List<string>{ "miyu (1).jpg", "miyu (2).jpg", "miyu (3).jpg", "miyu (4).jpg" } }
            //};

            await Azure.putPerseonGroupCreate(group_id, group_name);

            // 印出所有 PersonGroup 名稱，以檢查是否成功建立
            await getPersonGroupList();
        }

        async Task postPersonCreate(string group_id, string person_name)
        {            
            await Azure.postPersonCreate(group_id, person_name);

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            await getPersonList(group_id);
        }

        async Task postPersonAddFace(string group_id, string person_id, string file_name)
        {
            await Azure.postPersonAddFace(group_id, person_id, file_name);
        }

        async Task postPersonGroupTrain(string group_id)
        {
            await Azure.postPersonGroupTrain(group_id);
        }

        async Task deletePersonDelete(string group_id, string person_id)
        {
            await Azure.deletePersonDelete(group_id, person_id);
        }

        async Task getPersonList(string group_id)
        {
            PersonList list = await Azure.getPersonList(group_id);
            Debug.Log(list);
        }

        async Task deletePersonGroupDelete(string group_id)
        {
            await Azure.deletePersonGroupDelete(group_id);
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
