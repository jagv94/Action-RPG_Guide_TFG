using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenu;

    [SerializeField]
    private GameObject settingsMenu;

    [SerializeField]
    private GameObject gameMenu;

    [SerializeField]
    private XRRayInteractor rayInteractor;

    [SerializeField]
    private XRInteractorLineVisual lineVisual;

    private void Start()
    {
        ShowMainMenu();
        EnableLaserPointer(true);
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        gameMenu.SetActive(false);
    }

    public void ShowSettingsMenu()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        gameMenu.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void EnableLaserPointer(bool enable)
    {
        rayInteractor.enabled = enable;
        lineVisual.enabled = enable;
    }
}