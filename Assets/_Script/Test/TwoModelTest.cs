using ETLab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoModelTest : MonoBehaviour
{
    public Player player;
    float max_x_distance = 0f;
    float max_y_distance = 0f;
    float max_z_distance = 0f;

    // Start is called before the first frame update
    void Start()
    {
        player.resetInitPos();
        Debug.Log(string.Format("[TwoModelTest] Start | InitPos: {0}", (Vector3)player.getInitPos()));
    }

    // Update is called once per frame
    void Update()
    {
        max_x_distance = Mathf.Max(max_x_distance, player.getDistanceX());
        max_y_distance = Mathf.Max(max_y_distance, player.getDistanceY());
        max_z_distance = Mathf.Max(max_z_distance, player.getDistanceZ());
        Debug.Log(string.Format("[TwoModelTest] Update | max_x_distance: {0:F8}", max_x_distance));
        Debug.Log(string.Format("[TwoModelTest] Update | max_y_distance: {0:F8}", max_y_distance));
        Debug.Log(string.Format("[TwoModelTest] Update | max_z_distance: {0:F8}", max_z_distance));        
    }
}
