using UnityEngine;
using UnityEngine.UI;

public class UIInteractionLogger_Game : MonoBehaviour
{
    UIInteractionLogger instance;

    private void Start()
    {
        instance = UIInteractionLogger.Instance;
    }

    public void ButtonPressedRef(Button clickedObject)
    {
        
        if (instance != null)
        {
            instance.ButtonPressed(clickedObject);
        }
    }

    public void RegisterSettingsVisitRef()
    {
        if (instance != null)
        {
            instance.RegisterSettingsVisit();
        }
    }
}
