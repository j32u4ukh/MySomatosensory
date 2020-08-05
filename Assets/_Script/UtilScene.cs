using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETLab
{
    public class UtilScene : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            displayRecordData();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void displayRecordData()
        {
            string path = @"C:\Users\ETlab\Documents\Somatosensory\Data\9527\2020-08-04\IrtDemo2Scene-14-57-02-2630.txt";
            List<RecordData> records = RecordData.loadRecordDatas(path);
            HashSet<Pose> poses = new HashSet<Pose>();            

            foreach(RecordData record in records)
            {
                poses.Add(record.pose);
                //Debug.Log(record.toString());
            }

            List<Pose> pose_list = new List<Pose>(poses);
            int i, len = pose_list.Count;
            FloatList float_list;
            float acc;
            Pose pose;
            int count;

            for(i = 0; i < len; i++)
            {
                pose = pose_list[i];
                acc = 0;
                count = 0;

                foreach (RecordData record in records)
                {
                    if(record.pose == pose)
                    {
                        float_list = new FloatList(record.accuracy);
                        acc += float_list.geometricMean();
                        count++;
                    }
                }

                acc /= count;

                Debug.Log(string.Format("{0}: {1:F4}", pose, acc));
            }
        }
    }

}