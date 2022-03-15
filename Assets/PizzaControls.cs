using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PizzaControls : MonoBehaviour
{
    // Input variables

    Input input;

    Vector2 left_stick;
    Vector2 right_stick;
    Vector2 triggers;



    // Reference variables

    public GameObject UI;
    public GameObject l_stick_SFX;
    public GameObject r_stick_SFX;
    public GameObject partition;



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
    private int cursor_blink = 0;
    private string[,] letters = new string[7, 4];
    private bool just_typed = false;
    private string prev_char = "";
    private int tick_limit = 0;
    private int tick_limit2 = 0;
    private int prev_left = 0;


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
        letters[0, 0] = "a";
        letters[0, 2] = "b";
        letters[0, 3] = "c";
        letters[0, 1] = "d";
        letters[1, 0] = "e";
        letters[1, 2] = "f";
        letters[1, 3] = "g";
        letters[1, 1] = "h";
        letters[2, 0] = "i";
        letters[2, 2] = "j";
        letters[2, 3] = "k";
        letters[2, 1] = "l";
        letters[3, 0] = "m";
        letters[3, 2] = "n";
        letters[3, 3] = "o";
        letters[3, 1] = "p";
        letters[4, 0] = "q";
        letters[4, 2] = "r";
        letters[4, 3] = "s";
        letters[4, 1] = "t";
        letters[5, 0] = "u";
        letters[5, 2] = "v";
        letters[5, 3] = "w";
        letters[5, 1] = "x";
        letters[6, 0] = "y";
        letters[6, 2] = "z";
        letters[6, 3] = " ";
        letters[6, 1] = "<";
    }

    private void Update()
    {
        int slice_sel = get_joystick_sel(true);
        int letter_sel = get_joystick_sel(false);

        if (prev_left != slice_sel)
        {
            if (tick_limit2 >= 4)
            {
                tick_limit2 = 0;
                Instantiate(r_stick_SFX);
            }
            prev_left = slice_sel;
        }

        if (!prev_char.Equals(get_char()))
        {
            // returning to neutral
            if (get_char().Length > 0)
            {
                // backspace
                if (tick_limit >= 4)
                {
                    tick_limit = 0;
                    Instantiate(l_stick_SFX);
                }

                if (get_char().Equals("<"))
                    backspace();
                else
                    add_char(get_char());
            }
        }

        prev_char = get_char();
        if (tick_limit < 100) tick_limit++;
        if (tick_limit2 < 100) tick_limit2++;

        int i = 0;
        while (i < 7)
        {
            Transform slice = UI.transform.GetChild(4).GetChild(0).GetChild(0).GetChild(i);
            if (i == slice_sel)
            {
                slice.GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 0.5f, 0.75f);
                for (int j = 0; j < 4; j++)
                {
                    Transform letter = slice.GetChild(j);
                    if (j == letter_sel && get_joystick_angle(false) >= 0)
                        letter.GetComponent<UnityEngine.UI.Text>().color = new Color(0f, 0.8f, 1f);
                    else
                        letter.GetComponent<UnityEngine.UI.Text>().color = new Color(1f, 1f, 1f);
                }
            }
            else
            {
                slice.GetComponent<UnityEngine.UI.Image>().color = new Color(0.2f, 0.2f, 0.2f);
                for (int j = 0; j < 4; j++)
                {
                    Transform letter = slice.GetChild(j);
                    letter.GetComponent<UnityEngine.UI.Text>().color = new Color(0.6f, 0.6f, 0.6f);
                }
            }

            i++;
        }

        #region update textbox

        cursor_blink++;
        if (cursor_blink >= 60) cursor_blink = 0;

        string new_text = text_box;
        new_text = text_box + (cursor_blink < 40 ? "|" : "");
        UI.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = new_text;

        #endregion
    }

    void A()
    {
        print("A");
    }

    void B()
    {
        print("B");
    }

    void X()
    {
        sfx_clear.Play();
        text_box = "";
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
        print("D left");
    }

    void D_right()
    {
        print("D right");
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

    // HELPER METHODS

    float get_joystick_angle(bool left)
    {
        Vector2 stick = left ? left_stick : right_stick;

        // up is 0, right is 90, down is 180, left is 270
        if (Mathf.Abs(stick.x) + Mathf.Abs(stick.y) < 0.95f) return -1;
        float angle = Mathf.Atan(stick.y / stick.x) * Mathf.Rad2Deg;

        if (stick.x < 0) angle += 180;
        else if (stick.y < 0 && stick.x >= 0) angle += 360;

        angle = 90 - angle;
        if (angle < 0) angle += 360;
        return angle / 360;
    }

    int get_joystick_sel(bool left)
    {
        if (left)
        {
            return (int)(get_joystick_angle(true) * 7);
        }
        else
        {
            float r_angle = get_joystick_angle(false) + 0.125f;
            if (r_angle >= 1) r_angle -= 1;
            return (int)(r_angle * 4);
        }
    }

    void add_char(string c)
    {
        text_box = text_box + c;
        cursor_blink = 0;
    }

    void backspace()
    {
        if (text_box.Length > 0)
        {
            sfx_backspace.Play();
            text_box = text_box.Substring(0, text_box.Length - 1);
            cursor_blink = 0;
        }
    }

    string get_char()
    {
        if (get_joystick_sel(true) < 0 || get_joystick_sel(false) < 0)
            return "";

        return letters[(int)get_joystick_sel(true), (int)get_joystick_sel(false)];
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
