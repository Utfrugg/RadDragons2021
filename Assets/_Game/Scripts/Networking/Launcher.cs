using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class Launcher : MonoBehaviourPunCallbacks
{
    // Store these settings in a separate class or struct perhaps
    private string gameVersion = "14";
    private int lobbiesFound = 0;
    private string createLobbyName = "Epic Room";

    [SerializeField] private GameObject lobbies;
    [SerializeField] private TextMeshProUGUI progressLabel;
    [SerializeField] private GameObject lobbyEntry;

    private List<GameObject> lobbyEntries = new List<GameObject>();

    void Awake()
    {
        if (PhotonNetwork.IsConnected)
        {
            var players = GameObject.FindObjectsOfType<PlayerController>();
            for (int i = players.Length - 1; i >= 0; i--)
            {
                Destroy(players[i].gameObject);
            }

            PhotonNetwork.Disconnect();
        }

        Cursor.lockState = CursorLockMode.None;

        PhotonNetwork.AutomaticallySyncScene = true;

        progressLabel.text = "Connecting...";
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();

        
    }

    //public void ConnectToPhoton()
    //{
    //    if (PhotonNetwork.IsConnected)
    //    {
    //        JoinRoom();
    //    }
    //}

    public void CreateAndJoinRoom()
    {
        JoinRoom(createLobbyName);
    }

    //should be called on button press
    public void JoinRoom(string roomName)
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



    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        progressLabel.text = "Connected!";

    }

    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> roomList)
    {
        base.OnLobbyStatisticsUpdate(roomList);

        progressLabel.text = "Fetching Lobbies";

        Debug.Log("Getting lobby stats");

        if (lobbiesFound > 0)
        {
            foreach (var lob in lobbyEntries)
            {
                Destroy(lob);
            }

            lobbyEntries.Clear();
            lobbiesFound = 0;
        }

        foreach (var room in roomList)
        {
            if (room.Name == "")
            {
                continue;
            }

            var lob = GameObject.Instantiate(lobbyEntry, lobbies.GetComponentInChildren<ScrollRect>().content.transform);
            lob.GetComponent<LobbyEntry>().Init(room.Name);
            lob.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25 - 50 * lobbiesFound);
            lobbyEntries.Add(lob);
            lobbiesFound++;
        }

        progressLabel.text = "Connected!";
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
            Debug.Log("We load The LobbyRoom");

            PhotonNetwork.LoadLevel("LobbyRoom");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        progressLabel.text = "Offline";
    }

    public void SetRoomName(string pname)
    {
        createLobbyName = pname;
    }
}
