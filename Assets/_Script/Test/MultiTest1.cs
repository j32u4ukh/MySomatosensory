using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETLab
{
    public class MultiTest1 : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            float val = 3.3456f;
            float power = 2;
            float scale = (float)Math.Pow(10.0, power);
            float floor = (float)(Math.Floor(val * scale) / scale);
            Debug.Log(floor);

        }

        // Update is called once per frame
        void Update()
        {

        }

    }
}
