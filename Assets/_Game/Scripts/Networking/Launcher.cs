using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Store these settings in a separate class or struct perhaps

    private string roomName = "EpicRoom";
    private string gameVersion = "1";

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void ConnectToPhoton()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void JoinRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Connected! Creating/Joining room " + roomName);

            //Here is where max players and such can be passed along
            RoomOptions roomOptions = new RoomOptions();
            TypedLobby typedLobby = new TypedLobby(roomName, LobbyType.Default);
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
        }
    }

    public void LoadArena()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            PhotonNetwork.LoadLevel("MainArena");
        }
        else
        {
            
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        JoinRoom();

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        string players = "";
        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            players += player.Value.NickName + " ";
        }
        Debug.Log(PhotonNetwork.CurrentRoom.Name + ": " + players);
    }
}
