using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ETLab
{
    // TODO: 紀錄各動作的"無動作正確率" -> 多動作偵測時可推測當前在做何種動作，進而對它調整門檻值
    // TODO: "無動作正確率"亦可決定各動作的最低門檻調整下限，或許應該高於"無動作正確率"? 才不至於站著不動都通過
    // TODO: 先以 RecodeReply 場景錄製 Pose.None 的骨架資訊，再計算於其他動作下的正確率
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

        void computeAsymptomaticAccuray(Pose base_pose, Pose compare_pose)
        {
            // 建構當下不會讀取數據，實際需要使用到前再讀取就好
            MultiPosture mp = new MultiPosture();

            mp.onMultiPostureLoaded.AddListener((Pose pose_type) => {
                Utils.log(string.Format("實際動作 {0} 多比對標準載入完成", pose_type));
            });

            List<List<Posture>> base_multi_postures = mp.getMultiPostures(base_pose);
            List<List<Posture>> compare_multi_postures = mp.getMultiPostures(compare_pose);

        }
    }

}