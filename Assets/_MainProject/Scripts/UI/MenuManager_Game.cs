using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class VRMenuManager_Game : MonoBehaviour
{
    [SerializeField] private SceneAsset mainMenuScene;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private List<GameObject> pauseSubMenu;
    public InputActionReference menuKey;

    private void OpenMenu()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);

            if (pauseMenu.activeSelf)
            {
                foreach (GameObject submenu in pauseSubMenu)
                {
                    if(submenu.name == "Inventory Menu" || submenu.name == "PM Buttons")
                    {
                        submenu.SetActive(true);
                    }
                    else
                    {
                        submenu.SetActive(false);
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        if (menuKey != null)
        {
            menuKey.action.performed += ctx => OpenMenu();
        }
    }

    private void OnDisable()
    {
        if (menuKey != null)
        {
            menuKey.action.performed -= ctx => OpenMenu();
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(mainMenuScene.name);
    }
}