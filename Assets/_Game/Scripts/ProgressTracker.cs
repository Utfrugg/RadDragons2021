using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ProgressTracker : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int scoreNeededToWin = 5;

    private List<PlayerController> players = new List<PlayerController>();

    private int winningPlayer = 0;
    private int winningScore = 0;
    private bool playerWon = false;

    [SerializeField] private EndScreenUI endScreenUi;

    // Start is called before the first frame update
    public void PlayerInnit(PlayerController player)
    {
        players.Add(player);
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerWon)
        {
            if (winningPlayer != 0)
            {
                Debug.Log("<color=green> Winning player: " + winningPlayer + "</color>");
                PlayerWon();
                playerWon = true;
            }
        }

        if (!playerWon)
        {
            foreach (var pl in players)
            {
                Debug.Log("<color=green> Playerid: " + pl.photonView.Controller.NickName + " Score: " + pl.score +
                          "</color>");
                if (pl.score >= scoreNeededToWin)
                {

                    winningPlayer = pl.photonView.ControllerActorNr;
                    winningScore = pl.score;
                }
            }
        }
    }

    private void PlayerWon()
    {
        foreach (var pl in players)
        {
            if (pl.photonView.ControllerActorNr == winningPlayer)
            {
                endScreenUi.Show(pl.photonView.Controller.NickName, winningScore);
                break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(winningPlayer);
            stream.SendNext(winningScore);
        }
        else
        {
            winningPlayer = (int) stream.ReceiveNext();
            winningScore = (int) stream.ReceiveNext();
        }
    }
}
