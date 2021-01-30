using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Store these settings in a separate class or struct perhaps

    private string roomName = "EpicRoom";
    private string gameVersion = "2";

    [SerializeField] private GameObject controlPanel;
    [SerializeField] private GameObject progressLabel;

    private bool isConnecting;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }

    public void ConnectToPhoton()
    {
        progressLabel.SetActive(true);
        controlPanel.SetActive(false);

        PhotonNetwork.GameVersion = gameVersion;

        if (PhotonNetwork.IsConnected)
        {
            JoinRoom();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            isConnecting = true;
        }
        
    }

    public void JoinRoom()
    {
        if (isConnecting)
        {
            Debug.Log("Connected! Creating/Joining room " + roomName);

            //Here is where max players and such can be passed along
            RoomOptions roomOptions = new RoomOptions();
            TypedLobby typedLobby = new TypedLobby(roomName, LobbyType.Default);
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, typedLobby);
            isConnecting = false;
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

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("We load 'Room for 1'");

            PhotonNetwork.LoadLevel("Room for 1");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        progressLabel.SetActive(false);
        controlPanel.SetActive(true);
    }
}
