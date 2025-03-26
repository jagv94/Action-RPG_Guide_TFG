using UnityEngine;
using UnityEngine.EventSystems;
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

    private Camera mainCamera;

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

        mainCamera = Camera.main;
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
            return;

        // Intento de detectar si el clic fue sobre un botón o un UI interactivo
        if (IsPointerOverUI())
        {
            UserEventLogger.Instance.LogEvent("click_ui", "Click on UI");
            return;
        }

        // Si no hay interacción con UI, realizar un raycast para detectar otros objetos interactivos
        if (!IsPointingAtObject())
        {
            // Si no hay colisión con un objeto interactivo, es un clic en el aire
            UserEventLogger.Instance.LogEvent("click_air", "Click on air");
        }
    }

    private bool IsPointerOverUI()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width / 2, Screen.height / 2) // Centro de la pantalla
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private bool IsPointingAtObject()
    {
        if (mainCamera == null)
            return false;

        Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        return Physics.Raycast(ray, out _, 10f); // Detecta objetos hasta una distancia de 10 unidades
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