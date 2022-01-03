using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybControls : MonoBehaviour
{
    // Input variables

    Input input;
    PlayerInput playerInput;

    Vector2 left_stick;
    Vector2 right_stick;
    Vector2 triggers;



    // Reference variables

    public GameObject UI;
    public GameObject stick_SFX;
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
    private bool caps = false;
    private int keyboard = 0; // 0 : letter, 1 : nums, 2 : symb, 3 : color
    private float color = 0;
    private float keyb_pos_anim = 1;
    private float keyb_size_anim = 1;
    private float space_size_anim = 1;
    private char[] symbols = { '.', ',', '!', '?', ':', ';', '-', '_', ')', '(', '@', '#', '%', '&', '\'', '*' };
    private float[,] key_weights = new float[3, 26];
    private int prev_char = -1;
    private int tick_limit = 0;
    private int cursor = 0;
    private int cursor_blink = 0;


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
            Transform text_label = UI.transform.GetChild(4).GetChild(0).GetChild(2).GetChild(i);
            text_label.GetComponent<UnityEngine.UI.Text>().text = "" + (char)(97 + i);
            text_label.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 315 + new Vector3(0, 5, 0);
            angle -= Mathf.PI * 2 / num_keys;
        }

        // NUMBERS setup
        num_keys = 26;
        angle = Mathf.PI * 0.5f;

        for (int i = 0; i < num_keys; i++)
        {
            Transform text_label = UI.transform.GetChild(4).GetChild(1).GetChild(2).GetChild(i);
            text_label.GetComponent<UnityEngine.UI.Text>().text = "" + i;
            text_label.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 315 + new Vector3(0, 5, 0);
            angle -= Mathf.PI * 2 / num_keys;
        }

        // SYMBOLS Setup
        num_keys = 26;
        angle = Mathf.PI * 0.5f;

        for (int i = 0; i < num_keys; i++)
        {
            Transform text_label = UI.transform.GetChild(4).GetChild(3).GetChild(2).GetChild(i);
            text_label.GetComponent<UnityEngine.UI.Text>().text = "" + symbols[(i)];
            text_label.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 315 + new Vector3(0, 5, 0);
            angle -= Mathf.PI * 2 / num_keys;
        }

        #endregion

        // key_weights initalization
        for (int j = 0; j < key_weights.GetLength(1); j++)
            key_weights[0, j] = 1.0f / 26.0f;

        for (int j = 0; j < key_weights.GetLength(1); j++)
            key_weights[1, j] = 1.0f / 10.0f;

        for (int j = 0; j < key_weights.GetLength(1); j++)
            key_weights[2, j] = 1.0f / 16.0f;
    }

    private void Update()
    {
        #region keyboard refresh
        Transform current_keyboard = UI.transform.GetChild(4).GetChild(keyboard);

        if (keyboard < 3)
        {
            for (int i = 0; i < current_keyboard.childCount; i++)
            {
                int letter = 0;
                if (get_joystick_angle() < 0)
                {
                    // space is selected
                    letter = -1;
                    UI.transform.GetChild(5).GetComponent<UnityEngine.UI.Image>().color = new Color(0.5f, 1, 1);
                }
                else
                {
                    // one of the letters is selected
                    letter = (int)(get_joystick_angle() * UI.transform.GetChild(4).GetChild(keyboard).childCount);
                    UI.transform.GetChild(5).GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1);
                }
            }

            // letters
            float angle = 90 - key_weights[0, 0] / 2;
            for (int j = 0; j < 26; j++)
            {
                UI.transform.GetChild(4).GetChild(0).GetChild(1).GetChild(j).rotation = Quaternion.Euler(0, 0, angle);
                angle -= 360 * key_weights[0, j];
            }

            // numbers
            for (int j = 0; j < 10; j++)
                UI.transform.GetChild(4).GetChild(0).GetChild(1).GetChild(j).rotation =
                    Quaternion.Euler(0, 0, 360 * key_weights[1, j]);

            // symbols
            for (int j = 0; j < 16; j++)
                UI.transform.GetChild(4).GetChild(0).GetChild(1).GetChild(j).rotation =
                    Quaternion.Euler(0, 0, 360 * key_weights[2, j]);

            // TEMP NEEDLE
            if (get_joystick_angle() >= 0)
                UI.transform.GetChild(7).rotation = Quaternion.Euler(0, 0, -360 * get_joystick_angle());
        }
        else
        {
            // color wheel needle
            if (get_joystick_angle() >= 0)
                current_keyboard.GetChild(2).rotation = Quaternion.Euler(0, 0, -360 * get_joystick_angle());
        }
        #endregion

        #region LT RT shift
        if (keyboard == 0)
        {
            if ((triggers.x > 0.9f || triggers.y > 0.9f) && !caps)
            {
                sfx_shift.Play();
                sfx_unshift.Stop();
                caps = true;
                for (int i = 0; i < current_keyboard.childCount; i++)
                {
                    Transform letter_tile = current_keyboard.GetChild(i).GetChild(0);
                    letter_tile.GetComponent<UnityEngine.UI.Text>().text =
                        letter_tile.GetComponent<UnityEngine.UI.Text>().text.ToUpper();
                }
            }
            else if (triggers.x < 0.8f && triggers.y < 0.8f && caps)
            {
                sfx_unshift.Play();
                sfx_shift.Stop();
                caps = false;
                for (int i = 0; i < current_keyboard.childCount; i++)
                {
                    Transform letter_tile = current_keyboard.GetChild(i).GetChild(0);
                    letter_tile.GetComponent<UnityEngine.UI.Text>().text =
                        letter_tile.GetComponent<UnityEngine.UI.Text>().text.ToLower();
                }
            }
        }
        #endregion

        #region keyb swap animation
        keyb_pos_anim = keyb_pos_anim * 0.95f + 0.05f;
        keyb_size_anim = keyb_size_anim * 0.7f + 0.3f;
        space_size_anim = space_size_anim * 0.7f + 0.3f;

        for (int i = 0; i < UI.transform.GetChild(4).childCount; i++)
        {
            int to_pos = i - keyboard + 2;
            if (to_pos < 0) to_pos = 0;
            if (to_pos > 4) to_pos = 4;

            // move the keyboard
            UI.transform.GetChild(4).GetChild(i).position =
                UI.transform.GetChild(3).GetChild(to_pos).position * keyb_pos_anim
                + UI.transform.GetChild(4).GetChild(i).position * (1 - keyb_pos_anim);
            // resize the keyboard
            UI.transform.GetChild(4).GetChild(i).localScale =
                new Vector3(0.8f + 0.2f * (keyboard == i ? keyb_size_anim : 1 - keyb_size_anim),
                            0.8f + 0.2f * (keyboard == i ? keyb_size_anim : 1 - keyb_size_anim),
                            1);
        }
        // move and resize the spacebar
        int space_to_pos = keyboard == 3 ? 1 : 2;
        UI.transform.GetChild(5).position =
            UI.transform.GetChild(3).GetChild(space_to_pos).position * keyb_pos_anim
            + UI.transform.GetChild(5).position * (1 - keyb_pos_anim);
        UI.transform.GetChild(5).localScale =
                new Vector3(0.8f + 0.2f * (keyboard == 3 ? 1 - space_size_anim : space_size_anim),
                            0.8f + 0.2f * (keyboard == 3 ? 1 - space_size_anim : space_size_anim),
                            1);
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
        if (cursor_blink >= 380) cursor_blink = 0;

        string new_text = text_box;
        new_text = text_box.Substring(0, cursor)
            + (cursor_blink < 180 ? "|" : "")
            + text_box.Substring(cursor, text_box.Length-cursor);
        UI.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = new_text;

        #endregion

        #region update color sample

        if (keyboard == 3 && get_joystick_angle() >= 0)
            UI.transform.GetChild(4).GetChild(3).GetChild(1).GetComponent<UnityEngine.UI.Image>().color
                = Color.HSVToRGB(get_joystick_angle(), 1, 1);

        #endregion
    }

    void A()
    {
        if (keyboard < 3)
        {
            sfx_letter_sel.Play();
            text_box = text_box.Substring(0, cursor) + get_joystick_character() + text_box.Substring(cursor, text_box.Length - cursor);
            cursor++;
            cursor_blink = 0;
        }
        else
        {
            if (get_joystick_angle() >= 0)
            {
                sfx_color_sel.Play();
                color = get_joystick_angle();
                UI.transform.GetChild(6).GetComponent<UnityEngine.UI.Image>().color = Color.HSVToRGB(color, 1, 1);
            }
        }
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
        set_keyboard(-1);
    }

    void RB()
    {
        set_keyboard(1);
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
        // up is a, right is like f, down is m, left is u)
        float angle = get_joystick_angle();
        if (angle < 0) return ' ';

        if (keyboard == 0)
            return (char)((int)(65 + (caps ? 0 : 32) + angle * 26));
        else if (keyboard == 1)
            return (char)((int)(48 + angle * 10));
        else if (keyboard == 2)
            return symbols[(int)(angle * symbols.Length)];
        return ' ';
    }

    void set_keyboard(int delta_keyboard)
    {
        if (delta_keyboard == -1) {
            if (keyboard == 0) return;
            sfx_wheel_swap_left.Play();
        }
        if (delta_keyboard == 1) {
            if (keyboard == 3) return;
            sfx_wheel_swap_right.Play();
        }

        if (keyboard == 3 && delta_keyboard == -1 || keyboard == 2 && delta_keyboard == 1) space_size_anim = 0;
        keyboard += delta_keyboard;
        keyb_pos_anim = 0;
        keyb_size_anim = 0;
    }
}
