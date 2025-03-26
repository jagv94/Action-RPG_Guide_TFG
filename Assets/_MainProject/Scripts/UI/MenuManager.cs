using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private SceneAsset gameScene;

    public void StartGame()
    {
        SceneManager.LoadScene(gameScene.name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}