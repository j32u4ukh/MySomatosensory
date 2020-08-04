using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTest1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float pi = (float)Math.Round(Math.PI, 2);
        Debug.Log(string.Format("pi: {0:F8}", pi));
    }

    // Update is called once per frame
    void Update()
    {

    }

}
