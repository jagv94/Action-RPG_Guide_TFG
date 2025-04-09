using UnityEngine;
using UnityEngine.SceneManagement;

public class VRMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName;

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.SceneAsset gameScene;

    private void OnValidate()
    {
        if (gameScene != null)
        {
            gameSceneName = gameScene.name;
        }
    }
#endif

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}