using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool gamePaused = false;
    //private Controls.PlayerActions playerControls;
    public GameObject mainPauseMenuUI;
    public GameObject backToMainMenuUI;
    public GameObject quitGameMenuUI;

    private void Start()
    {
        //playerControls = FindObjectOfType<InputController>().PlayerControls;
        //playerControls.PauseGame.performed += ctx => SetPaused();
    }

    void SetPaused()
    {
        if (gamePaused)
        {
            ResumeGame();
            mainPauseMenuUI.SetActive(false);
            backToMainMenuUI.SetActive(false);
            quitGameMenuUI.SetActive(false);
        }
        else
        {
            PauseGame();
            mainPauseMenuUI.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        mainPauseMenuUI.SetActive(false);
        backToMainMenuUI.SetActive(false);
        quitGameMenuUI.SetActive(false);
        Time.timeScale = 1f;
        gamePaused = false;
    }

    public void PauseGame()
    {
        mainPauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        gamePaused = true;
    }

    // Used to quit the game, will not work in editor
    public void QuitGame()
    {
        Application.Quit();
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene("Level_Concept_Gregor", LoadSceneMode.Single);
    }

    public void ReloadLevel()
    {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
    }


    public void Menu()
    {
        SceneManager.LoadScene("Level_MainMenu", LoadSceneMode.Single);
    }

}

