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
            displayRecordData(@"C:\Users\ETlab\Documents\Somatosensory\Data\9527\2020-08-11\IrtDemo2Scene-11-09-58-8691.txt");
            //displayStandardData(@"D:\Unity Projects\MySomatosensory\Assets\StreamingAssets\MovementData\RaiseTwoHands\RecordRePlay-15-21-09-3302.txt");
            //sampleStandardData(Pose.RaiseTwoHands);
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 將指定的紀錄中，各動作的正確率作幾何平均並依序印出
        /// </summary>
        void displayRecordData(string path)
        {
            List<RecordData> records = RecordData.loadRecordDatas(path);
            HashSet<Pose> poses = new HashSet<Pose>();            

            foreach(RecordData record in records)
            {
                poses.Add(record.pose);
                //Debug.Log(record.ToString());
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

        /// <summary>
        /// 將作為比對標準的紀錄數據印出，該數據不會有正確率或門檻值(但還是要有該欄位，才能被解析為 RecordData)，
        /// 因為需要的只有骨架位置。
        /// </summary>
        void displayStandardData(string path)
        {
            RecordData record = RecordData.loadRecordData(path);
            Debug.Log(string.Format("date: {0}, pose: {1}", record.date, record.pose));

            List<Posture> posture_list = record.posture_list;
            Debug.Log(string.Format("#posture_list: {0}", posture_list.Count));
            
            for(int i = 0; i < 5; i++)
            {
                Debug.Log(string.Format("index: {0}, posture: {1}", i, posture_list[i].skeletons.ToString()));
            }
        }

        void sampleStandardData(Pose pose)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "MovementData", pose.ToString());
            string[] files = Directory.GetFiles(path);
            RecordData record;

            foreach (string file in files)
            {
                // 不包含 .meta 的才是真正的檔案
                if (!file.Contains(".meta"))
                {
                    Debug.Log(string.Format("file: {0}", file));
                    record = RecordData.loadRecordData(file);
                    record.posture_list = Utils.sampleList(record.posture_list, n_sample: 5);
                    record.reSave(file);
                }
            }
        }
    }

}