using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ETLab
{
    public class MultiFaceIdentify
    {
        /*
        [
            {
                "faceId": "93dec0ee-4bcb-43e0-847a-eb66bbec5892",
                "candidates": 
                [
                    {
                        "personId": "bd5acccd-990c-4915-8685-1b80d984960f",
                        "confidence": 0.90247
                    }
                ]
            }, 
            {
                "faceId": "8c4db702-99f1-4e7c-9b58-76e7a7892733",
                "candidates": []
            }
        ]
        */
        public List<FaceIdentify> multi_identify = new List<FaceIdentify>();
        int n_identified = -1;

        public int getIdentifiedNumber()
        {
            if(n_identified == -1)
            {
                n_identified = 0;

                foreach (FaceIdentify identify in multi_identify)
                {
                    if (identify.getCandidatesNumber() > 0)
                    {
                        n_identified++;
                    }
                }
            }

            return n_identified;
        }

        public void resetIdentifiedNumber()
        {
            n_identified = -1;
        }

        public List<string> getPersonIds()
        {
            List<string> ids = new List<string>();

            int len = multi_identify.Count;

            if(len == 0)
            {
                Debug.LogError("偵測到的人臉數量為 0");                
            }
            else
            {
                int i;

                for(i = 0; i < len; i++)
                {
                    ids.Add(multi_identify[i].getPersonId());
                }
            }

            return ids;
        }

        /// <summary>
        /// 原始輸入的 Json 數據沒有 multi_identify 標籤，但這是套件轉換時所必須的，故自行添加
        /// </summary>
        /// <param name="json_data"></param>
        public static MultiFaceIdentify loadData(string json_data)
        {
            json_data = string.Format("{{'multi_identify': {0}}}", json_data);
            return JsonConvert.DeserializeObject<MultiFaceIdentify>(json_data);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            int i, len = multi_identify.Count;

            for(i = 0; i < len; i++)
            {
                if(i != 0)
                {
                    sb.Append("\n");
                }

                sb.Append(multi_identify[i].ToString());
            }

            return sb.ToString();
        }
    }

    public class FaceIdentify
    {
        /*
        {
            "faceId": "93dec0ee-4bcb-43e0-847a-eb66bbec5892",
            "candidates": 
            [
                {
                    "personId": "bd5acccd-990c-4915-8685-1b80d984960f",
                    "confidence": 0.90247
                }
            ]
        }
        */
        public string faceId;
        public List<Candidates> candidates = new List<Candidates>();

        public int getCandidatesNumber()
        {
            return candidates.Count;
        }

        public string getPersonId()
        {
            if (candidates.Count > 0)
            {
                return candidates[0].personId;
            }
            else
            {
                Debug.LogError("候選人數為 0");
                return "";
            }
        }

        public override string ToString()
        {
            if(candidates.Count > 0)
            {
                return string.Format("face id: {0}, match: ({1})", faceId, candidates[0]);
            }
            else
            {
                return string.Format("face id: {0}, match: null", faceId);
            }
            
        }
    }

    public class Candidates
    {
        /*
        {
            "personId": "bd5acccd-990c-4915-8685-1b80d984960f",
            "confidence": 0.90247
        }
         */
        public string personId;
        public float confidence;

        public override string ToString()
        {
            return string.Format("person id: {0}, confidence: {1:F4}", personId, confidence);
        }
    }

    public class IdentifyRequestBody
    {
        public string personGroupId;
        public List<string> faceIds = new List<string>();

        // 最多返回多少位可能候選人
        public int maxNumOfCandidatesReturned = 1;
        public float confidenceThreshold = 0.5f;

        public void add(string face_id)
        {
            faceIds.Add(face_id);
        }

        public void add(List<string> face_ids)
        {
            foreach(string face_id in face_ids)
            {
                faceIds.Add(face_id);
            }
        }
    }

}
