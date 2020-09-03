using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityAsync;

namespace ETLab
{
    public class MultiTest1 : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //string json_data = "{'people': [{\"personId\":\"0336c22a-cba0-4bb4-b8ac-b31321dcf7ff\",\"persistedFaceIds\":['id1'],\"name\":\"miyu1\"},{\"personId\":\"4c2f25f3-3191-4430-bdcf-65c56cd97535\",\"persistedFaceIds\":['id2'],\"name\":\"miyu2\"}]}";
            string json_data = "[{\"personId\":\"0336c22a-cba0-4bb4-b8ac-b31321dcf7ff\",\"persistedFaceIds\":[],\"name\":\"miyu1\"},{\"personId\":\"4c2f25f3-3191-4430-bdcf-65c56cd97535\",\"persistedFaceIds\":[],\"name\":\"miyu2\"}]";
            PersonList list = PersonList.loadData(json_data);
            Debug.Log(list.ToString());
            //Azure.initConfigData();

            //string file_name = "detection1.jpg";
            //Debug.Log(string.Format("[JsonAzure] Start | file_name: {0}", file_name));

            //try
            //{
            //    makeAnalysisRequest(file_name);
            //}
            //catch (Exception e)
            //{
            //    Debug.LogError(string.Format("[JsonAzure] Start | {0}: {1}", e.GetType(), e.Message));
            //}

        }

        // Update is called once per frame
        void Update()
        {

        }

        async void makeAnalysisRequest(string file_name)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ConfigData.FACE_SUBSCRIPTION_KEY1);

            // Request parameters. A third optional parameter is "details".
            //string parameters = "returnFaceId=true&returnFaceLandmarks=false" +
            //    "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
            //    "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise&recognitionModel=recognition_02";

            string parameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=smile,emotion&recognitionModel=recognition_02";

            // Assemble the URI for the REST API Call.
            string uri = string.Format("{0}face/v1.0/detect?{1}", ConfigData.FACE_ENDPOINT, parameters);
            Debug.Log(string.Format("[JsonAzure] makeAnalysisRequest | uri: {0}", uri));

            HttpResponseMessage response;

            // Request body. Posts a locally stored JPEG image.
            byte[] bytes = await getImageBytes(file_name);

            Debug.Log(string.Format("[JsonAzure] makeAnalysisRequest | #bytes: {0}", bytes.Length));

            using (ByteArrayContent content = new ByteArrayContent(bytes))
            {
                // This example uses content type "application/octet-stream".
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                // Execute the REST API call.
                response = await client.PostAsync(uri, content);

                // Get the JSON response.
                string json_string = await response.Content.ReadAsStringAsync();
                Debug.Log(string.Format("[JsonAzure] makeAnalysisRequest | json_string:\n{0}", json_string));

                // Display the JSON response.
                string json = jsonParser(json_string);
                Debug.Log(string.Format("[JsonAzure] makeAnalysisRequest | Result\n{0}", json));
            }
        }

        async Task<byte[]> getImageBytes(string file_name)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "image", file_name);
            Debug.Log(string.Format("[JsonAzure] getImageBytes(path: {0})", path));

            byte[] bytes;
            using (FileStream file_stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[file_stream.Length];
                await file_stream.ReadAsync(bytes, 0, (int)file_stream.Length);
            }

            return bytes;
        }

        //async Task<byte[]> androidLoadImageAsync(string path)
        //{
        //    UnityWebRequest request = UnityWebRequest.Get(path);
        //    Debug.Log(string.Format("[LocalTest] androidLoadImageAsync | path: {0}", path));
        //    await request.SendWebRequest();
        //    Debug.Log("[LocalTest] androidLoadImageAsync | request sent.");

        //    while (!request.isDone && !request.isNetworkError && !request.isHttpError) { }

        //    if (!request.isNetworkError && !request.isHttpError)
        //    {
        //        return request.downloadHandler.data;
        //    }

        //    return null;
        //}

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
