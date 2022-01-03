using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suicide : MonoBehaviour
{
    private int lifetime = 30;

    void Update()
    {
        lifetime--;
        if (lifetime <= 0) Destroy(this.gameObject);
    }
}
