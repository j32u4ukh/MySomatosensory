using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ETLab
{
    public static class Azure
    {
        public const string RecognitionModel01 = "recognition_01";
        public const string RecognitionModel02 = "recognition_02";
        public const string RecognitionModel03 = "recognition_03";

        public static void initConfigData()
        {
            using (StreamReader reader = new StreamReader(ConfigData.config_path))
            {
                string line;
                string[] content;

                while ((line = reader.ReadLine()) != null)
                {
                    content = line.Split(' ');

                    switch (content[0])
                    {
                        case "FACE_SUBSCRIPTION_KEY1":
                            ConfigData.FACE_SUBSCRIPTION_KEY1 = content[1];
                            break;
                        case "FACE_SUBSCRIPTION_KEY2":
                            ConfigData.FACE_SUBSCRIPTION_KEY2 = content[1];
                            break;
                        case "FACE_ENDPOINT":
                            ConfigData.FACE_ENDPOINT = content[1];
                            break;
                        default:
                            Debug.LogError(string.Format("[Azure] loadConfigData | key: {0}, value: {1}", content[0], content[1]));
                            break;
                    }
                }
            }
        }

        public static string newGroupId()
        {
            return Guid.NewGuid().ToString();
        }

        public static async Task<byte[]> getImageBytes(string file_name)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "image", file_name);
            Debug.Log(string.Format("[Azure] getImageBytes(path: {0})", path));

            byte[] bytes;
            using (FileStream file_stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[file_stream.Length];
                await file_stream.ReadAsync(bytes, 0, (int)file_stream.Length);
            }

            return bytes;
        }

        /// <summary>
        /// 取得 PersonGroup 清單，以 PersonGroupList 的形式回傳
        /// </summary>
        /// <returns></returns>
        public static async Task<PersonGroupList> getPersonGroupList()
        {
            QueryParameter query = new QueryParameter(service: AzureService.PersonGroupList);
            query.add("top", "1000");
            query.add("returnRecognitionModel", "false");

            using (UnityWebRequest request = UnityWebRequest.Get(uri: query.ToString()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);

                await request.SendWebRequest();

                // Get the JSON response.
                string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    json_string = request.downloadHandler.text;
                    PersonGroupList list = PersonGroupList.loadData(json_string);
                    return list;
                }
            }

            return null;
        }

        /// <summary>
        /// 偵測並返回圖片中的人臉及位置 by UnityWebRequest
        /// </summary>
        /// <param name="file_name">要偵測的檔案的名稱(含副檔名)</param>
        /// <returns></returns>
        public static async Task<FaceDetects> postFaceDetect(string file_name)
        {
            QueryParameter query = new QueryParameter(service: AzureService.Detect);
            query.add("returnFaceId", true);
            query.add("returnFaceLandmarks", false);
            query.add("recognitionModel", Azure.RecognitionModel01);
            query.add("returnRecognitionModel", false);
            query.add("detectionModel", "detection_01");

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);
                request.SetRequestHeader("Content-Type", "application/octet-stream");

                byte[] bytes = await Azure.getImageBytes(file_name);
                request.uploadHandler.contentType = "application/octet-stream";
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                // Get the JSON response.
                string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    json_string = request.downloadHandler.text;
                    FaceDetects face_detects = FaceDetects.loadData(json_string);
                    return face_detects;
                }
            }

            return null;
        }

        /// <summary>
        /// 若該 Person 存在於此 PersonGroup，則返回該 Person
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <param name="person_id">Person 的唯一對應碼</param>
        /// <returns></returns>
        public static async Task<Person> getPerson(string group_id, string person_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.Person, group_id: group_id, person_id: person_id);

            using (UnityWebRequest request = UnityWebRequest.Get(uri: query.ToString()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                // Get the JSON response.
                string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    json_string = request.downloadHandler.text;
                    Person person = JsonConvert.DeserializeObject<Person>(json_string);
                    return person;
                }
            }

            return null;
        }

        public static async Task<List<Person>> postFaceIdentify(string group_id, string file_name)
        {
            FaceDetects face_detects = await postFaceDetect(file_name);

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            QueryParameter query = new QueryParameter(service: AzureService.Identify);

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY2);
                request.SetRequestHeader("Content-Type", "application/json");

                IdentifyRequestBody request_body = new IdentifyRequestBody
                {
                    personGroupId = group_id
                };
                request_body.add(face_detects.getFaceIds());

                string json_data = JsonConvert.SerializeObject(request_body);
                byte[] bytes = Encoding.UTF8.GetBytes(json_data);

                request.uploadHandler.contentType = "application/json";
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                // Get the JSON response.
                string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    json_string = request.downloadHandler.text;

                    MultiFaceIdentify multi_identify = MultiFaceIdentify.loadData(json_string);
                    List<string> ids = multi_identify.getPersonIds();
                    Debug.Log(string.Format("發現 {0} 張人臉", ids.Count));
                    List<Person> people = new List<Person>();

                    foreach (string person_id in ids)
                    {
                        Person person = await getPerson(group_id, person_id);
                        people.Add(person);

                        // Limit TPS (避免請求頻率過高) 3000
                        Debug.Log(string.Format("Limit TPS"));
                        await Task.Delay(3000);
                    }

                    Debug.Log(string.Format("辨識結束"));
                    return people;
                }
            }

            return null;
        }
    }

    public class QueryParameter
    {
        private StringBuilder sb;
        private bool initialized;

        public QueryParameter(string service)
        {
            string query = string.Format("{0}/face/v1.0/{1}?", ConfigData.FACE_ENDPOINT, service);
            sb = new StringBuilder(query);
        }

        public QueryParameter(AzureService service, string group_id = "", string person_id = "")
        {
            string query = string.Format("{0}/face/v1.0/{1}", ConfigData.FACE_ENDPOINT,
                Utils.getDescription(service.ToString(), typeof(AzureService)));

            if (service == AzureService.Person)
            {
                query = string.Format("{0}/{1}/persons/{2}", query, group_id, person_id);
            }

            sb = new StringBuilder(query);
        }

        public void add(string key, string value)
        {
            if (initialized)
            {
                sb.Append("&");
            }
            else
            {
                initialized = true;
                sb.Append("?");
            }

            sb.Append(string.Format("{0}={1}", key, value));
        }

        public void add(string key, bool value)
        {
            if (value)
            {
                add(key, value: "true");
            }
            else
            {
                add(key, value: "false");
            }
        }

        public void addList(string key, params string[] values)
        {
            StringBuilder sb = new StringBuilder("[");

            for (int i = 0; i < values.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(",");
                }

                sb.Append(values[i]);
            }

            sb.Append("]");

            add(key, value: sb.ToString());
        }

        public override string ToString()
        {
            return sb.ToString();
        }

    }
}
