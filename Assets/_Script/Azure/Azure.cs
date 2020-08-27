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
        public const string DetectionModel01 = "detection_01";
        public const string DetectionModel02 = "detection_02";

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

        /// <summary>
        /// 取得 API 所需的 bytes 數據
        /// </summary>
        /// <param name="source_name">要偵測的資源的名稱(若為檔案則須含副檔名；若為 orbbec 則抓取感測器的畫面)</param>
        /// <returns></returns>
        public static async Task<byte[]> getImageBytes(string source_name)
        {
            byte[] bytes;

            if (source_name.ToLower().Equals("orbbec"))
            {
                KinectManager manager = KinectManager.Instance;
                Texture2D texture = manager.GetUsersClrTex2D();
                bytes = texture.EncodeToJPG();
            }
            else
            {
                string path = Path.Combine(Application.streamingAssetsPath, "image", source_name);
                Debug.Log(string.Format("[Azure] getImageBytes(path: {0})", path));

                using (FileStream file_stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bytes = new byte[file_stream.Length];
                    await file_stream.ReadAsync(bytes, 0, (int)file_stream.Length);
                }
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
        /// <param name="source_name">要偵測的資源的名稱(若為檔案則須含副檔名；若為 orbbec 則抓取感測器的畫面)</param>
        /// <returns></returns>
        public static async Task<FaceDetects> postFaceDetect(string source_name)
        {
            QueryParameter query = new QueryParameter(service: AzureService.Detect);
            query.add("returnFaceId", true);
            query.add("returnFaceLandmarks", false);
            query.add("recognitionModel", RecognitionModel01);
            query.add("returnRecognitionModel", false);
            query.add("detectionModel", DetectionModel01);

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);
                request.SetRequestHeader("Content-Type", "application/octet-stream");

                byte[] bytes = await getImageBytes(source_name);
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
            Debug.Log(face_detects);

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            QueryParameter query = new QueryParameter(service: AzureService.Identify);
            Debug.Log(string.Format("uri: {0}", query.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);
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
                    Debug.Log(string.Format("辨識出 {0} 張資料庫中的人臉", ids.Count));
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

        #region Create PerseonGroup
        // 似乎將它想得太難了，userData 既然是 (optional) 那就表示它並非創建時所必要的
        public static async Task createPersonGroup(string group_id, string group_name, Dictionary<string, List<string>> people)
        {
            Debug.Log(string.Format("開始建立 PerseonGroup(group_id: {0}, group_name: {1})", group_id, group_name));
            Debug.Log(string.Format("開始初始化"));
            await putPerseonGroupCreate(group_id: group_id, group_name: group_name);

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            foreach (string person_name in people.Keys)
            {
                Debug.Log(string.Format("開始建立 Person: {0}", person_name));
                await postPersonCreate(group_id: group_id, person_name: person_name);
                Debug.Log(string.Format("完成建立 Person: {0}", person_name));

                // Limit TPS (避免請求頻率過高) 3000
                Debug.Log(string.Format("Limit TPS"));
                await Task.Delay(3000);
            }

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            // 取得 PersonList，用於獲得各個 Person 的 person_id
            PersonList list = await getPersonList(group_id);
            string person_id;
            bool found_person;

            foreach (Person person in list.people)
            {
                found_person = false;

                foreach (string name in people.Keys)
                {
                    // 根據 name 取得 Person，再利用 Person 取得 personId
                    if (name.Equals(person.name))
                    {
                        Debug.LogWarning(string.Format("開始 Person {0} 照片添加", person.name));
                        found_person = true;
                        person_id = person.personId;

                        foreach(string source_name in people[name])
                        {
                            await postPersonAddFace(group_id: group_id, person_id: person_id, source_name: source_name);

                            // Limit TPS (避免請求頻率過高) 3000
                            Debug.Log(string.Format("Limit TPS"));
                            await Task.Delay(3000);
                        }

                        break;
                    }
                }

                if (found_person)
                {
                    Debug.LogWarning(string.Format("完成 Person {0} 照片添加", person.name));
                }
                else
                {
                    Debug.LogWarning(string.Format("Person {0} 沒有找到", person.name));
                }
            }

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            await postPersonGroupTrain(group_id: group_id);

            Debug.Log(string.Format("完成建立 PerseonGroup(group_id: {0}, group_name: {1})", group_id, group_name));
        }

        /// <summary>
        /// 建立 PersonGroup(空殼，只有名稱)
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <param name="group_name">PersonGroup 名稱</param>
        /// <returns></returns>
        public static async Task putPerseonGroupCreate(string group_id, string group_name)
        {
            QueryParameter query = new QueryParameter(service: AzureService.PersonGroup, group_id: group_id);

            PersonGroupRequestBody body = new PersonGroupRequestBody()
            {
                name = group_name
            };

            Debug.Log(string.Format("PersonGroupRequestBody: {0}", body.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Put(uri: query.ToString(), bodyData: body.toBytes()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY2);
                request.SetRequestHeader("Content-Type", "application/json");

                request.uploadHandler.contentType = "application/json";
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    Debug.Log(string.Format("成功建立 PerseonGroup, name: {0}, group_id: {1}", group_name, group_id));
                }
            }
        }

        // TODO: 目前無法排除重複 person_name 的情形
        public static async Task postPersonCreate(string group_id, string person_name)
        {
            QueryParameter query = new QueryParameter(service: AzureService.CreatePerson, group_id: group_id);
            Debug.Log(string.Format("uri: {0}", query.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY2);
                request.SetRequestHeader("Content-Type", "application/json");

                PersonCreateRequestBody body = new PersonCreateRequestBody() {
                    name = person_name
                };

                request.uploadHandler.contentType = "application/json";
                request.uploadHandler = new UploadHandlerRaw(body.toBytes());
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                //string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    //json_string = request.downloadHandler.text;
                    //Debug.Log(string.Format(string.Format("json_string: {0}", json_string)));
                    Debug.Log(string.Format(string.Format("新增 {0} 至 PersonGroup: {1}", person_name, group_id)));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <param name="person_id">Person 的唯一對應碼</param>
        /// <param name="source_name">要偵測的資源的名稱(若為檔案則須含副檔名；若為 orbbec 則抓取感測器的畫面)</param>
        /// <returns></returns>
        public static async Task postPersonAddFace(string group_id, string person_id, string source_name)
        {
            QueryParameter query = new QueryParameter(service: AzureService.PersonAddFace, group_id: group_id, person_id: person_id);
            query.add("detectionModel", DetectionModel01);
            //Debug.Log(string.Format("uri: {0}", query.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);
                request.SetRequestHeader("Content-Type", "application/octet-stream");

                byte[] bytes = await getImageBytes(source_name);
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
                    Debug.Log(string.Format("json_string: {0}", json_string));
                }
            }
        }

        public static async Task postPersonGroupTrain(string group_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.TrainPersonGroup, group_id: group_id);
            Debug.Log(string.Format("uri: {0}", query.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Post(uri: query.ToString(), formData: new WWWForm()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);

                request.uploadHandler.contentType = "application/octet-stream";
                request.downloadHandler = new DownloadHandlerBuffer();

                await request.SendWebRequest();

                // Get the JSON response.
                //string json_string;
                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    //json_string = request.downloadHandler.text;
                    Debug.Log(string.Format("完成 PersonGroup {0} 的訓練", group_id));

                    // Limit TPS (避免請求頻率過高) 3000
                    Debug.Log(string.Format("Limit TPS"));
                    await Task.Delay(3000);

                    string training_status = await getPersonGroupGetTrainingStatus(group_id);
                    Debug.Log(string.Format("training_status: {0}", training_status));
                }
            }
        }

        public static async Task<string> getPersonGroupGetTrainingStatus(string group_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.GetTrainingStatus, group_id: group_id);
            Debug.Log(string.Format("uri: {0}", query.ToString()));

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
                    return json_string;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 根據 group_id 取得該 PersonGroup 當中所有人
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <returns></returns>
        public static async Task<PersonList> getPersonList(string group_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.PersonList, group_id: group_id);
            query.add("top", 1000);
            //Debug.Log(string.Format("uri: {0}", query.ToString()));

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
                    PersonList list = PersonList.loadData(json_string);
                    return list;
                }
            }

            return null;
        }

        /// <summary>
        /// 根據 person_id 與 group_id 刪除對應的 Person
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <param name="person_id">Person 的唯一對應碼</param>
        /// <returns></returns>
        public static async Task deletePersonDelete(string group_id, string person_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.DeletePerson, group_id: group_id, person_id: person_id);
            Debug.Log(string.Format("uri: {0}", query.ToString()));

            using (UnityWebRequest request = UnityWebRequest.Delete(uri: query.ToString()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY2);

                await request.SendWebRequest();

                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    Debug.Log(string.Format("成功刪除 Person: {0}", person_id));
                }
            }

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            //
            PersonList list = await getPersonList(group_id);
            Debug.Log(list);
        }

        /// <summary>
        /// 根據 group_id 刪除 PersonGroup
        /// </summary>
        /// <param name="group_id">PersonGroup 的唯一對應碼</param>
        /// <returns></returns>
        public static async Task deletePersonGroupDelete(string group_id)
        {
            QueryParameter query = new QueryParameter(service: AzureService.PersonGroup, group_id: group_id);

            using(UnityWebRequest request = UnityWebRequest.Delete(uri: query.ToString()))
            {
                // 設置 Header
                // https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.SetRequestHeader.html
                request.SetRequestHeader("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY2);

                await request.SendWebRequest();

                while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

                if (!request.isNetworkError && !request.isHttpError)
                {
                    Debug.Log(string.Format("成功刪除 PersonGroup: {0}", group_id));
                }
            }

            // Limit TPS (避免請求頻率過高) 3000
            Debug.Log(string.Format("Limit TPS"));
            await Task.Delay(3000);

            PersonGroupList list = await getPersonGroupList();
            Debug.Log(list);
        }
        #endregion
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

            switch (service)
            {
                case AzureService.CreatePerson:
                case AzureService.PersonList:
                    query = string.Format("{0}/{1}/persons", query, group_id);
                    break;
                case AzureService.Person:
                case AzureService.DeletePerson:
                    query = string.Format("{0}/{1}/persons/{2}", query, group_id, person_id);
                    break;
                case AzureService.PersonAddFace:
                    query = string.Format("{0}/{1}/persons/{2}/persistedFaces", query, group_id, person_id);
                    break;
                case AzureService.TrainPersonGroup:
                    query = string.Format("{0}/{1}/train", query, group_id);
                    break;
                case AzureService.GetTrainingStatus:
                    query = string.Format("{0}/{1}/training", query, group_id);
                    break;
                case AzureService.PersonGroup:
                    query = string.Format("{0}/{1}", query, group_id);
                    break;
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

        public void add(string key, int value)
        {
            add(key, value.ToString());
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
