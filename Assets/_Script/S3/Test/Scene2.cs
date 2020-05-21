using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2 : MonoBehaviour
{
    Dont1 d1;
    Dont2 d2;

    // Start is called before the first frame update
    void Start()
    {
        GameObject dont = GameObject.Find("Dont");
        d1 = dont.GetComponent<Dont1>();
        d2 = dont.GetComponent<Dont2>();

        d1.printValue();
        d2.add(7);
        d1.printValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
