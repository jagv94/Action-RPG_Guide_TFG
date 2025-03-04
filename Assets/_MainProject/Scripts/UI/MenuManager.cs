using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private SceneAsset gameScene;
    [SerializeField] private Canvas bodySettingsMenu;
    [SerializeField] private Slider playerHeight;
    [SerializeField] private TextMeshProUGUI heightText;

    private void Update()
    {
        if (bodySettingsMenu != null && bodySettingsMenu.isActiveAndEnabled)
        {
            heightText.text = playerHeight.value.ToString();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameScene.name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}