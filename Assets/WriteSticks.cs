using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class WriteSticks : MonoBehaviour
{
    private StreamWriter file_writer;
    public GameObject camera;
    private int frame = 0;

    private void Start()
    {
        // create the text file
        string pathName = Application.streamingAssetsPath + "/Stick_Logs/";

        Directory.CreateDirectory(pathName);

        int i = 0;

        while (File.Exists(pathName + "log_" + i + ".csv"))
        {
            i++;
        }

        file_writer = new StreamWriter(pathName + "log_" + i + ".csv", true);
        file_writer.WriteLine("frame,text_box,left_x,left_y,right_x,right_y");
    }

    private void Update()
    {
        Vector2 l = Vector2.zero;
        Vector2 r = Vector2.zero;
        string text_box = "";

        if (camera.GetComponent<ZoomControls>() != null)
        {
            l = camera.GetComponent<ZoomControls>().get_left_stick();
            r = camera.GetComponent<ZoomControls>().get_right_stick();
            text_box = camera.GetComponent<ZoomControls>().get_textbox();
        }

        file_writer.WriteLine($"{frame},{text_box},{l.x},{l.y},{r.x},{r.y}");

        frame += 1;
    }
}
