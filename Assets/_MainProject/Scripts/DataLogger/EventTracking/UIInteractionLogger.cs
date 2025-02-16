using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class UIInteractionLogger : MonoBehaviour
{
    public static UIInteractionLogger Instance;

    [Header("Input Actions (Usando los controles preconfigurados)")]
    public InputActionProperty leftSelectAction;
    public InputActionProperty rightSelectAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (DetectClick())
        {
            DetectClickType();
        }
    }

    private bool DetectClick()
    {
        bool leftPressed = leftSelectAction.action.WasPressedThisFrame();
        bool rightPressed = rightSelectAction.action.WasPressedThisFrame();
        bool mousePressed = Mouse.current?.leftButton.wasPressedThisFrame ?? false;

        return leftPressed || rightPressed || mousePressed;
    }

    private void DetectClickType()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current?.position.ReadValue() ?? Vector2.zero
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;
            if (clickedObject.GetComponent<UnityEngine.UI.Button>())
            {
                UserEventLogger.Instance.LogEvent("click_button", clickedObject.name);
            }
            else
            {
                UserEventLogger.Instance.LogEvent("click_ui", clickedObject.name);
            }
        }
        else
        {
            UserEventLogger.Instance.LogEvent("click_air", "None");
        }
    }
}