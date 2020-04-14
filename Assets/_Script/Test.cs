using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Test : MonoBehaviour {

	void Start () {        
        int n_sample = 39, length = 34, start, end;
        if (n_sample > length)
        {
            n_sample = length;
        }

        float sample_size = (float)length / n_sample;
        print(string.Format("sample_size: {0:F4}", sample_size));

        System.Random random = new System.Random();
        for (int i = 0; i < n_sample; i++)
        {
            start = (int)(sample_size * i);
            end = (int)(start + sample_size);
            int rand = random.Next(start, end);
            print(string.Format("start: {0}, end: {1} -> rand: {2}", start, end, rand));
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
