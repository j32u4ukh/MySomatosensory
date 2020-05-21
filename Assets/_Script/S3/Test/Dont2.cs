using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dont2 : MonoBehaviour
{
    public int val = 0;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void add(int v)
    {
        val += v;
        Debug.Log(string.Format("[Dont2] val: {0}", val));
    }
}
