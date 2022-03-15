using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPhrases : MonoBehaviour
{
    string[] phrases;
    public bool next = false;
    public string current_phrase = "";
    private int phrase_pointer = 0;

    private void Start()
    {
        TextAsset file = (TextAsset)Resources.Load("phrases2");
        phrases = file.ToString().Split('\n');
    }
    void Update()
    {
        current_phrase = phrases[phrase_pointer];
        gameObject.GetComponent<UnityEngine.UI.Text>().text = current_phrase;

        if (next)
        {
            phrase_pointer++;
            next = false;
        }
    }
}
