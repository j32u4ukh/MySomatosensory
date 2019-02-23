using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        float[] nums = new float[10];
        nums[0] = 4.5f;
        nums[1] = 8.7f;
        float[] arr = sliceArray(nums, 0, 1);
        for (int i = 0; i < arr.Length; i++) {
            print(arr[i]);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
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
