using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cam_rotate : MonoBehaviour
{
    private float angle = 0;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        transform.LookAt(new Vector3(0, 0, 0));
        angle += 0.001f;
    }
}
