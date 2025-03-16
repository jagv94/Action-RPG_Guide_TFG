using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private SceneAsset gameScene;
    [SerializeField] private SceneAsset MainMenuScene;
    [SerializeField] private Canvas bodySettingsMenu;
    [SerializeField] private Slider playerHeight;
    [SerializeField] private TextMeshProUGUI heightText;

    private void Update()
    {
        if (bodySettingsMenu != null && bodySettingsMenu.isActiveAndEnabled &&
            playerHeight  != null && heightText != null)
        {
            heightText.text = playerHeight.value.ToString();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameScene.name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MainMenuScene.name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}