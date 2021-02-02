using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text scoreText;

    [SerializeField] private GameObject hostControls;
    [SerializeField] private GameObject waitingText;
    [SerializeField] private GameObject panel;



    public void QuitToMenu()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void Show(string playerName, int score)
    {
        Cursor.lockState = CursorLockMode.None;
        panel.SetActive(true);
        winnerText.text = playerName + " has won!";
        scoreText.text = "With a score of: " + score;

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            waitingText.SetActive(false);
            hostControls.SetActive(true);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(0);
        }
    }

    public void RestartGame()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("IslandScene");
        }
    }
}
