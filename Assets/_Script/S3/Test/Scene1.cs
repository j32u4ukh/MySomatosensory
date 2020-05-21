using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene1 : MonoBehaviour
{
    public Dont1 d1;
    public Dont2 d2;

    // Start is called before the first frame update
    void Start()
    {
        d1.printValue();
        d2.add(7);
        d1.printValue();

        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SceneManager.LoadScene("_Scenes/Scene2");
        }
    }
}
