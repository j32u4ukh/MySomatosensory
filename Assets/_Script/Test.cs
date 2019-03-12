using System;
using System.IO;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        fileInfoTest(@"C:\Users\etlab\Documents\Somatosensory\RaiseRightHand (3)_renamed.txt");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void fileInfoTest(string path) {
        FileInfo file_info = new FileInfo(path);
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
            DirectoryInfo di = new DirectoryInfo(dir);
            foreach (FileInfo file_info in di.GetFiles())
            {
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
                        file_info.MoveTo(new_path);
                    }
                }
            }
        }
        catch(Exception e) {
            print(e.Message);
        }
    }


    float[] sliceArray(float[] array, int start, int end) {
        int len = end - start + 1, i;
        float[] new_array = new float[len];
        for (i = 0; i < len; i++) {
            new_array[i] = array[start + i];
        }
        return new_array;
    }

    void printNumbers(params int[] numbers)
    {
        for (int i = 0; i < numbers.Length; i++)
        {
            print(numbers[i]);
        }
    }

    void printEnum() {
        string[] array = Enum.GetNames(typeof(Pose));
        Array arr = Enum.GetValues(typeof(Pose));

        for (int i = 0; i < arr.Length; i++)
        {
            print(arr.GetValue(i));
        }
    }
}
