using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public Graphics graphicsSettings;
    public Optimization optimizationSettings;

    public void SaveAllSettings()
    {
        if (graphicsSettings != null)
        {
            graphicsSettings.SaveSettings();
        }

        if (optimizationSettings != null)
        {
            optimizationSettings.SaveSettings();
        }

        Debug.Log("All settings saved.");
    }
}