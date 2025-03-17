using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIInteractionLogger : MonoBehaviour
{
    public static UIInteractionLogger Instance;

    public InputActionReference leftSelectAction;
    public InputActionReference rightSelectAction;
    public InputActionReference leftSelectActionUI;
    public InputActionReference rightSelectActionUI;

    public int SettingsVisits { get; private set; } = 0;

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
        if (LoggerManager.Instance.Logger && DetectClick())
        {
            DetectClickType();
        }
    }

    private bool DetectClick()
    {
        bool leftPressed = leftSelectAction.action.WasPressedThisFrame() || leftSelectActionUI.action.WasPressedThisFrame();
        bool rightPressed = rightSelectAction.action.WasPressedThisFrame() || rightSelectActionUI.action.WasPressedThisFrame();

        return leftPressed || rightPressed;
    }

    private void DetectClickType()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        UserEventLogger.Instance.LogEvent("click_air_or_ui", "Click on air or UI");
    }

    public void RegisterSettingsVisit()
    {
        if (!LoggerManager.Instance.Logger)
        {
            return;
        }

        SettingsVisits++;
    }

    public void ButtonPressed(Button clickedObject)
    {
        if (!LoggerManager.Instance.Logger && clickedObject != null)
        {
            return;
        }

        UserEventLogger.Instance.LogEvent("click_button", clickedObject.name);
    }
}