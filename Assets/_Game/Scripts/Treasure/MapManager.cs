using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MapManager : MonoBehaviourPunCallbacks
{
    Dictionary<int, PlayerController> dick;

    void Start()
    {
        PlayerController[] allPlayersInScene = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayersInScene) {
            dick.Add(player.photonView.ControllerActorNr, player);
        }
    }

    void Update()
    {
        
    }
}
