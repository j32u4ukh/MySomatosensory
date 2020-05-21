using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dont1 : MonoBehaviour
{
    public Dont2 d2;
    int num = 0;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void printValue()
    {
        num++;
        Debug.Log(string.Format("[Dont1] val: {0}, num: {1}", d2.val, num));
    }
}
