using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiTest2 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(string.Format("[{0}] Start", gameObject.name));
    }

    private void FixedUpdate()
    {
        Debug.Log(string.Format("[{0}] FixedUpdate", gameObject.name));
    }


    // Update is called once per frame
    void Update()
    {
        Debug.Log(string.Format("[{0}] Update", gameObject.name));
    }

    private void OnDestroy()
    {
        Debug.Log(string.Format("[{0}] OnDestroy", gameObject.name));
    }
}
