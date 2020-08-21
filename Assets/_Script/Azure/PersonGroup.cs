using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    public class PersonGroupList
    {
        /*
        [   
            {
                "personGroupId": "5ce45c41-d387-4afc-8241-279da096e27e",
                "name": "5ce45c41-d387-4afc-8241-279da096e27e"
            },
            {
                "personGroupId": "9fecd399-d395-488d-be66-7123165fcd9e",
                "name": "9fecd399-d395-488d-be66-7123165fcd9e"
            },
            {
                "personGroupId": "b7b2f64c-2653-4285-beb1-80282fc71e99",
                "name": "b7b2f64c-2653-4285-beb1-80282fc71e99"
            },
            {
                "personGroupId": "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7",
                "name": "c5c2dc65-8bb4-4da8-9da3-707e5184b8a7"
            }
        ]
        */

        public List<PersonGroup> groups;


        /// <summary>
        /// 原始輸入的 Json 數據沒有 groups 標籤，但這是套件轉換時所必須的，故自行添加
        /// </summary>
        /// <param name="json_data"></param>
        /// <returns></returns>
        public static PersonGroupList loadData(string json_data)
        {
            json_data = string.Format("{{'groups': {0}}}", json_data);
            return JsonConvert.DeserializeObject<PersonGroupList>(json_data);
        }

        public override string ToString()
        {
            return string.Format("找到 {0} 個 PersonGroup\n{1}", groups.Count, Utils.listToString(groups));
        }
    }

    public class PersonGroup
    {
        public string personGroupId;
        public string name;

        public static string getPersonServiceUri(string group_id, string person_id)
        {
            return string.Format("{0}/persongroups/{1}/persons/{2}?", ConfigData.FACE_ENDPOINT, group_id, person_id);
        }

        public override string ToString()
        {
            return personGroupId;
        }
    }

    public class Person
    {
        public string personId;
        public string name;
        public List<string> persistedFaceIds;

        public override string ToString()
        {
            return string.Format("person_id: {0}, name: {1}\npersisted face ids: {2}", personId, name, Utils.listToString(persistedFaceIds));
        }
    }
}