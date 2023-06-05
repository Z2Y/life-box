using System;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

[PrefabResource("Prefabs/ui/Joystick")]
public class JoyStickController : UIBase
{
    public bl_Joystick Joystick { get; private set; }
    public static JoyStickController Instance { get; private set; }
    public static bool isReady { get; private set;  }

    public KeyCodeButton[] buttons;

    private void Awake()
    {
        Joystick = GetComponent<bl_Joystick>();
        transform.SetParent(UIManager.Instance.screenRoot, false);
        Input.simulateMouseWithTouches = false;
        isReady = true;
    }

    public Vector2 offset => Joystick.GetOffset();

    public JoyStickButton GetButtonFor(KeyCode code)
    {
        foreach (var item in buttons)
        {
            if (item.code == code)
            {
                item.button.gameObject.SetActive(true);
                return item.button;
            }
        }
        return null;
    }

    public bl_Joystick GetJoystickFor(KeyCode code)
    {
        foreach (var item in buttons)
        {
            if (item.code == code)
            {
                item.button.gameObject.SetActive(true);
                return item.joystick;
            }
        }
        return null;
    }

    public static async UniTask LoadAsync()
    {
        Instance = await UIManager.Instance.FindOrCreateAsync<JoyStickController>() as JoyStickController;
    }
    
    [Serializable]
    public class KeyCodeButton
    {
        public KeyCode code;
        public JoyStickButton button;
        public bl_Joystick joystick;
    }
}

public static class bl_Joystick_Helper {
    public static Vector2 GetOffset(this bl_Joystick joystick)
    {
        return new Vector2(joystick.Horizontal, joystick.Vertical);
    }
}