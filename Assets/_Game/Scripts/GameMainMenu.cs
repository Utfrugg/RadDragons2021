using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMainMenu : MonoBehaviour
{
    public void Start()
    {
        Time.timeScale = 1;
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene("IslandScene", LoadSceneMode.Single);
        Debug.Log("Load Level");
    }

    public void ReloadLevel()
    {
        Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
    }


  public void Menu()
  {
      SceneManager.LoadScene("IslandScene", LoadSceneMode.Single);
      Debug.Log("Main Menu");
  }

    // Used to quit the game, will not work in editor
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit game");
    }
}
