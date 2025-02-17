using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private XRInteractorLineVisual lineVisual;
    [SerializeField] private SceneAsset gameScene;
    [SerializeField] private Canvas bodySettingsMenu;
    [SerializeField] private Slider playerHeight;
    [SerializeField] private TextMeshProUGUI heightText;

    private void Start()
    {
        EnableLaserPointer(true);
    }

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

    private void EnableLaserPointer(bool enable)
    {
        if (rayInteractor != null && lineVisual != null)
        {
            rayInteractor.enabled = enable;
            lineVisual.enabled = enable;
        }
    }
}