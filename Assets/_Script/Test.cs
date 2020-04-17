using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using S3;
using Pose = S3.Pose;
using System.Text;
using Newtonsoft.Json;

public class Test : MonoBehaviour {

	void Start () {
        MovementDatas datas = MovementDatas.loadMovementDatas();
        MovementData data;
        List<HumanBodyBones> comparing_parts;

        foreach (var key in datas.getKeys())
        {
            print(string.Format("Pose {0}", key));
            data = datas.get(key);
            comparing_parts = data.comparing_parts;

            foreach (var part in comparing_parts)
            {
                print(string.Format("{0}", part));
            }

            if (data.has_additional_condition)
            {
                print("With additional condition.");
            }
            else
            {
                print("Without additional condition.");
            }
        }

    }
	
	// Update is called once per frame
	void Update () {

    }

    void paramTest(params string[] words)
    {
        foreach(string word in words)
        {
            print(word);
        }
    }
    
    void fileInfoTest(string path) {
        // 給予路徑，取得檔案資訊
        FileInfo file_info = new FileInfo(path);
        // file_info.Name：取得檔名(包含副檔名)
        print(file_info.Name);
        string file_name = file_info.Name.Split('.')[0];
        string[] name_split = file_name.Split('_');
        string suffix = name_split[1];
        print("suffix:" + suffix);
    }

    void filesInDir(string dir)
    {
        try
        {
            // 取得資料夾 dir
            DirectoryInfo di = new DirectoryInfo(dir);
            // 利用 di.GetFiles() 取得資料夾內所有檔案
            foreach (FileInfo file_info in di.GetFiles())
            {
                // file_info.Name：取得檔名(包含副檔名)
                string file_name = file_info.Name.Split('.')[0];
                string[] name_split = file_name.Split('_');
                int len = name_split.Length;
                if (len.Equals(2))
                {
                    string suffix = name_split[1];
                    if (suffix.Equals("複製"))
                    {
                        print(file_name);
                        string new_path = Path.Combine(file_info.DirectoryName, string.Format("{0}_renamed.txt", name_split[0]));
                        // 將檔案移至新路徑(若路徑相同，但檔名不同，則效果等價於 rename)
                        file_info.MoveTo(new_path);
                    }
                }
            }
        }
        catch(Exception e) {
            print(e.Message);
        }
    }

    // 擷取陣列內容
    float[] sliceArray(float[] array, int start, int end) {
        int len = end - start + 1, i;
        float[] new_array = new float[len];
        for (i = 0; i < len; i++) {
            new_array[i] = array[start + i];
        }
        return new_array;
    }

    // 不定參數個數嘗試
    void printNumbers(params int[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            print(numbers[i]);
        }
    }

    // 取得 Enum 列舉值
    void printEnum() {
        string[] array = Enum.GetNames(typeof(Pose));
        Array arr = Enum.GetValues(typeof(Pose));

        for (int i = 0; i < arr.Length; i++)
        {
            print(arr.GetValue(i));
        }
    }
}
