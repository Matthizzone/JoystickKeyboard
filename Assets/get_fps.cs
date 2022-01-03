using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class get_fps : MonoBehaviour
{
    private void Update()
    {
        string fps = (1.0f / Time.smoothDeltaTime).ToString();
        gameObject.GetComponent<UnityEngine.UI.Text>().text = fps.Substring(0, fps.IndexOf('.') + 2) + " fps";
    }
}
