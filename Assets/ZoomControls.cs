using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZoomControls : MonoBehaviour
{
    // Input variables

    Input input;

    Vector2 left_stick;
    Vector2 right_stick;
    Vector2 triggers;



    // Reference variables

    public GameObject UI;
    public GameObject stick_SFX;



    // SFX

    private AudioSource sfx_letter_sel;
    private AudioSource sfx_wheel_swap_left;
    private AudioSource sfx_wheel_swap_right;
    private AudioSource sfx_color_sel;
    private AudioSource sfx_backspace;
    private AudioSource sfx_clear;
    private AudioSource sfx_shift;
    private AudioSource sfx_unshift;
    private AudioSource sfx_d_left;
    private AudioSource sfx_d_right;



    // Code Variables

    private string text_box = "";
    private bool caps = false;
    public float zoom_amount = 1; // range: [0, 1]
    private int prev_char = -1;
    private int tick_limit = 0;
    private int cursor = 0;
    private int cursor_blink = 0;
    private bool wheel_lock = false;
    private float wheel_anim = 0;
    private float letter_lock = 0;


    private void Awake()
    {
        #region hooking up buttons
        input = new Input();

        input.Gameplay.A.performed += ctx => A();
        input.Gameplay.B.performed += ctx => B();
        input.Gameplay.X.performed += ctx => X();
        input.Gameplay.Y.performed += ctx => Y();

        input.Gameplay.Start.performed += ctx => start();
        input.Gameplay.Select.performed += ctx => Select();

        input.Gameplay.LeftStickUp.performed += ctx => left_stick.y = ctx.ReadValue<float>();
        input.Gameplay.LeftStickUp.canceled += ctx => left_stick.y = 0;
        input.Gameplay.LeftStickDown.performed += ctx => left_stick.y = -ctx.ReadValue<float>();
        input.Gameplay.LeftStickDown.canceled += ctx => left_stick.y = 0;
        input.Gameplay.LeftStickLeft.performed += ctx => left_stick.x = -ctx.ReadValue<float>();
        input.Gameplay.LeftStickLeft.canceled += ctx => left_stick.x = 0;
        input.Gameplay.LeftStickRight.performed += ctx => left_stick.x = ctx.ReadValue<float>();
        input.Gameplay.LeftStickRight.canceled += ctx => left_stick.x = 0;

        input.Gameplay.RightStickUp.performed += ctx => right_stick.y = ctx.ReadValue<float>();
        input.Gameplay.RightStickUp.canceled += ctx => right_stick.y = 0;
        input.Gameplay.RightStickDown.performed += ctx => right_stick.y = -ctx.ReadValue<float>();
        input.Gameplay.RightStickDown.canceled += ctx => right_stick.y = 0;
        input.Gameplay.RightStickLeft.performed += ctx => right_stick.x = -ctx.ReadValue<float>();
        input.Gameplay.RightStickLeft.canceled += ctx => right_stick.x = 0;
        input.Gameplay.RightStickRight.performed += ctx => right_stick.x = ctx.ReadValue<float>();
        input.Gameplay.RightStickRight.canceled += ctx => right_stick.x = 0;

        input.Gameplay.LT.performed += ctx => triggers.x = ctx.ReadValue<float>();
        input.Gameplay.LT.canceled += ctx => triggers.x = 0;
        input.Gameplay.LB.performed += ctx => LB();
        input.Gameplay.RT.performed += ctx => triggers.y = ctx.ReadValue<float>();
        input.Gameplay.RT.canceled += ctx => triggers.y = 0;
        input.Gameplay.RB.performed += ctx => RB();

        input.Gameplay.DUp.performed += ctx => D_up();
        input.Gameplay.DDown.performed += ctx => D_down();
        input.Gameplay.DLeft.performed += ctx => D_left();
        input.Gameplay.DRight.performed += ctx => D_right();
        #endregion

        #region SFX and Music

        sfx_letter_sel = gameObject.AddComponent<AudioSource>();
        sfx_letter_sel.clip = Resources.Load<AudioClip>("SFX/A");
        sfx_letter_sel.playOnAwake = false;

        sfx_wheel_swap_left = gameObject.AddComponent<AudioSource>();
        sfx_wheel_swap_left.clip = Resources.Load<AudioClip>("SFX/swipe_left");
        sfx_wheel_swap_left.playOnAwake = false;

        sfx_wheel_swap_right = gameObject.AddComponent<AudioSource>();
        sfx_wheel_swap_right.clip = Resources.Load<AudioClip>("SFX/swipe_right");
        sfx_wheel_swap_right.playOnAwake = false;

        sfx_color_sel = gameObject.AddComponent<AudioSource>();
        sfx_color_sel.clip = Resources.Load<AudioClip>("SFX/color_select");
        sfx_color_sel.playOnAwake = false;

        sfx_backspace = gameObject.AddComponent<AudioSource>();
        sfx_backspace.clip = Resources.Load<AudioClip>("SFX/B");
        sfx_backspace.playOnAwake = false;

        sfx_clear = gameObject.AddComponent<AudioSource>();
        sfx_clear.clip = Resources.Load<AudioClip>("SFX/X");
        sfx_clear.playOnAwake = false;

        sfx_shift = gameObject.AddComponent<AudioSource>();
        sfx_shift.clip = Resources.Load<AudioClip>("SFX/shift");
        sfx_shift.playOnAwake = false;

        sfx_unshift = gameObject.AddComponent<AudioSource>();
        sfx_unshift.clip = Resources.Load<AudioClip>("SFX/unshift");
        sfx_unshift.playOnAwake = false;

        sfx_d_left = gameObject.AddComponent<AudioSource>();
        sfx_d_left.clip = Resources.Load<AudioClip>("SFX/D-left");
        sfx_d_left.playOnAwake = false;

        sfx_d_right = gameObject.AddComponent<AudioSource>();
        sfx_d_right.clip = Resources.Load<AudioClip>("SFX/D-right");
        sfx_d_right.playOnAwake = false;

        #endregion
    }

    private void Start()
    {
        #region keyboard setup

        // LETTERS Setup
        int num_keys = 26;
        float angle = Mathf.PI * 0.5f;

        for (int i = 0; i < num_keys; i++)
        {
            Transform text_label = UI.transform.GetChild(3).GetChild(0).GetChild(2).GetChild(i);
            text_label.GetComponent<UnityEngine.UI.Text>().text = "" + (char)(97 + i);
            text_label.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 315 + new Vector3(0, 5, 0);
            angle -= Mathf.PI * 2 / num_keys;
        }

        #endregion
    }

    private void Update()
    {
        #region keyboard refresh
        Transform current_keyboard = UI.transform.GetChild(3).GetChild(0);

        for (int i = 0; i < current_keyboard.childCount; i++)
        {
            int letter = 0;
            if (get_joystick_angle() < 0)
            {
                // space is selected
                letter = -1;
                UI.transform.GetChild(4).GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 1, 1);
            }
            else
            {
                // one of the letters is selected
                letter = (int)(get_joystick_angle() * UI.transform.GetChild(3).GetChild(0).childCount);
                UI.transform.GetChild(4).GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
            }

            update_board(0); // LETTERS

            // TEMP NEEDLE
            if (get_joystick_angle() >= 0)
                UI.transform.GetChild(4).GetChild(0).rotation = Quaternion.Euler(0, 0, -360 * get_joystick_angle());
        }
        #endregion

        #region wheel tick
        if (prev_char != get_joystick_character())
        {
            if (tick_limit >= 3)
            {
                tick_limit = 0;
                Instantiate(stick_SFX);
            }
        }
        if (tick_limit < 100) tick_limit++;
        prev_char = get_joystick_character();

        #endregion

        #region update textbox

        cursor_blink++;
        if (cursor_blink >= 60) cursor_blink = 0;

        string new_text = text_box;
        new_text = text_box.Substring(0, cursor)
            + (cursor_blink < 40 ? "|" : "")
            + text_box.Substring(cursor, text_box.Length-cursor);
        UI.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = new_text;

        #endregion

        #region wheel locking

        if (!wheel_lock && (left_stick.x > 0.9f || left_stick.y > 0.9f || left_stick.x < -0.9f || left_stick.y < -0.9f))
        {
            wheel_lock = true;
            letter_lock = get_joystick_angle();
        }
        else if (wheel_lock && !(left_stick.x > 0.9f || left_stick.y > 0.9f || left_stick.x < -0.9f || left_stick.y < -0.9f))
        {
            wheel_lock = false;
        }

        wheel_anim += (wheel_lock ? 0.2f : -0.2f);
        if (wheel_anim > 1) wheel_anim = 1;
        if (wheel_anim < 0) wheel_anim = 0;

        #endregion
    }

    void A()
    {
        sfx_letter_sel.Play();
        text_box = text_box.Substring(0, cursor) + get_joystick_character() + text_box.Substring(cursor, text_box.Length - cursor);
        cursor++;
        cursor_blink = 0;
    }

    void B()
    {
        if (text_box.Length > 0 && cursor > 0)
        {
            sfx_backspace.Play();
            text_box = text_box.Substring(0, cursor-1) + text_box.Substring(cursor, text_box.Length - cursor);
            cursor--;
            cursor_blink = 0;
        }
    }

    void X()
    {
        sfx_clear.Play();
        text_box = "";
        cursor = 0;
        cursor_blink = 0;
    }

    void Y()
    {
        print("Y down");
    }

    void D_down()
    {
        print("D down");
    }

    void D_up()
    {
        print("D up");
    }

    void D_left()
    {
        if (cursor > 0)
        {
            sfx_d_left.Play();
            cursor--;
            cursor_blink = 0;
        }
    }

    void D_right()
    {
        if (cursor < text_box.Length)
        {
            sfx_d_right.Play();
            cursor++;
            cursor_blink = 0;
        }
    }

    void LB()
    {
        print("LB");
    }

    void RB()
    {
        print("RB");
    }

    void start()
    {
        print("Start down");
    }

    void Select()
    {
        print("Select down");
    }

    private void OnEnable()
    {
        input.Gameplay.Enable();
    }

    private void OnDisable()
    {
        input.Gameplay.Disable();
    }

    public Vector2 get_left_stick()
    {
        return left_stick;
    }

    public Vector2 get_right_stick()
    {
        return right_stick;
    }

    public string get_textbox()
    {
        return text_box;
    }

    // HELPER METHODS

    float get_joystick_angle()
    {
        // up is 0, right is 90, down is 180, left is 270
        if (Mathf.Abs(left_stick.x) + Mathf.Abs(left_stick.y) < 0.5f) return -1;
        float angle = Mathf.Atan(left_stick.y / left_stick.x) * Mathf.Rad2Deg;

        if (left_stick.x < 0) angle += 180;
        else if (left_stick.y < 0 && left_stick.x >= 0) angle += 360;

        angle = 90 - angle;
        if (angle < 0) angle += 360;
        return angle / 360;
    }

    char get_joystick_character()
    {
        float angle = get_joystick_angle();
        if (angle < 0) return ' ';
        angle = 1 - angle;

        Transform which_board = UI.transform.GetChild(3).GetChild(0);
        int num_children = which_board.GetChild(1).childCount;
        int child = 0;


        for (int j = 0; j < num_children; j++) {
            float left_angle = which_board.GetChild(1).GetChild(j).eulerAngles.z;
            float right_angle = (j != which_board.GetChild(1).childCount - 1 ?
                which_board.GetChild(1).GetChild(j + 1).eulerAngles.z
                : which_board.GetChild(1).GetChild(0).eulerAngles.z);

            left_angle /= 360;
            right_angle /= 360;

            if (Mathf.Abs(left_angle - right_angle) > 0.5f) right_angle -= 1;

            if (angle < left_angle && angle > right_angle)
            {
                child = j;
                break;
            }

            if (angle-1 < left_angle && angle-1 > right_angle)
            {
                child = j;
                break;
            }
        }

        return (char)(65 + (caps ? 0 : 32) + child);
    }

    void update_board(int which)
    {
        // First find a geometric regular arrangement, and then distort or magnify
        // by pulling all the partitions and labels away from the pointer.

        Transform which_board = UI.transform.GetChild(3).GetChild(which);
        int num_children = which_board.GetChild(1).childCount;

        float partition_angle = -180f / num_children; // degres   + is right // 360f / 26 / 2;
        float attraction_angle = letter_lock + 0.5f; // polar opposite of pointer
        if (attraction_angle > 1) attraction_angle -= 1f;

        for (int j = 0; j < num_children; j++)
        {
            // partition position
            float angle_temp = angle_minus(attraction_angle, partition_angle / 360f);
            float shifter = (wheel_anim * 0.6f) * (letter_lock < 0 ? 0 : angle_temp * zoom_amount)
                * -(Mathf.Abs(angle_temp) + 0.3f) * (Mathf.Abs(angle_temp) - 0.5f);

            which_board.GetChild(1).GetChild(j).localRotation =
                Quaternion.Euler(0, 0, -partition_angle - 3000f * shifter);
            partition_angle += 360f / num_children;

            // label position
            float left_angle = which_board.GetChild(1).GetChild(j).eulerAngles.z;
            float right_angle = (j != which_board.GetChild(1).childCount - 1 ? 
                which_board.GetChild(1).GetChild(j + 1).eulerAngles.z
                : which_board.GetChild(1).GetChild(0).eulerAngles.z);

            left_angle /= 360;
            right_angle /= 360;

            angle_temp = angle_plus(left_angle, right_angle) / 2;
            angle_temp = angle_temp * Mathf.PI * 2 + Mathf.PI / 2;

            which_board.GetChild(2).GetChild(j).localPosition =
                new Vector3(Mathf.Cos(angle_temp), Mathf.Sin(angle_temp), 0) * 315 + new Vector3(0, 5, 0);

            // label scaling
            float angle_size = angle_minus(left_angle, right_angle);
            which_board.GetChild(2).GetChild(j).GetComponent<UnityEngine.UI.Text>().fontSize = 
                (int)(Mathf.Min(Mathf.Max(2000 * angle_size, 0), 40));
        }
    }

    float angle_minus(float a, float b)
    {
        if (Mathf.Abs(a - b + 1) <= 0.5f) return a - b + 1;
        if (Mathf.Abs(a - b) <= 0.5f) return a - b;
        else return a - b - 1;
    }

    float angle_plus(float a, float b)
    {
        if (Mathf.Abs(a - b + 1) <= 0.5f) return a + b + 1;
        if (Mathf.Abs(a - b) <= 0.5f) return a + b;
        else return a + b - 1;
    }
}
