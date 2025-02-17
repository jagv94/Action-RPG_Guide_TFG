using UnityEngine;
using UnityEngine.UI;

public class LoggerManager : MonoBehaviour
{
    public static LoggerManager Instance { get; private set; }
    public bool Logger { get; private set; } = true;
    private const string LoggerPref = "LoggerIsActive";

    [SerializeField] private Button loggerButton;

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

        //if (PlayerPrefs.GetInt(LoggerPref, 1) == 0) // Si no existe se pone a 1 por defecto (1 = Activo).
        //{
        //    Logger = false;
        //}

        Logger = false;
    }

    public void ToggleLog()
    {
        if (loggerButton != null)
        {
            ColorBlock buttonColors = loggerButton.colors;

            if (!Logger)
            {
                Logger = true;
                buttonColors.normalColor = new Color(0, 191, 0);
                buttonColors.selectedColor = new Color(0, 191, 0);
                buttonColors.highlightedColor = new Color(0, 155, 0);
                buttonColors.pressedColor = new Color(61, 246, 72);
                loggerButton.colors = buttonColors;

                //PlayerPrefs.SetInt(LoggerPref, 1); // 1 = Activo.
                //PlayerPrefs.Save();
            }
            else
            {
                Logger = false;
                buttonColors.normalColor = new Color(191, 0, 0);
                buttonColors.selectedColor = new Color(191, 0, 0);
                buttonColors.highlightedColor = new Color(155, 0, 0);
                buttonColors.pressedColor = new Color(246, 61, 72);
                loggerButton.colors = buttonColors;

                //PlayerPrefs.SetInt(LoggerPref, 0); // 0 = Inactivo.
                //PlayerPrefs.Save();
            }
        }
    }
}
