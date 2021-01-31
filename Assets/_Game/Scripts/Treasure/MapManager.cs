using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class MapManager : MonoBehaviourPunCallbacks, IPunObservable
{
    Dictionary<int, PlayerController> dick = new Dictionary<int, PlayerController>();

    TreasureData p1Treasure;
    TreasureData p2Treasure;
    TreasureData p3Treasure;
    TreasureData p4Treasure;
    int activeMaps;
    public TreasureData[] treasureIndex;

    private TreasureSpawner treasureSpawn;
    public int shouldSpawnTreasureForPlayer = -1;

    private CreateMapTextures MapCaptureCam;

    void Start()
    {
        treasureIndex = new TreasureData[] { p1Treasure, p2Treasure, p3Treasure, p4Treasure};
        treasureSpawn = GameObject.FindObjectOfType<TreasureSpawner>();
        MapCaptureCam = GameObject.FindObjectOfType<CreateMapTextures>();

        TreasureMap[] allMaps = FindObjectsOfType<TreasureMap>();
        foreach (TreasureMap map in allMaps)
        {
            map.InitMapInnit();
        }

        PlayerController[] allPlayersInScene = FindObjectsOfType<PlayerController>();

        foreach (PlayerController player in allPlayersInScene) 
        {
            dick.Add(player.photonView.ControllerActorNr, player);
            Debug.Log("<color=yellow>Playername: " + player.photonView.Controller.NickName + " id: " + player.photonView.ControllerActorNr + "</color>");
        }

        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            foreach (var dickEntry in dick)
            {
                Debug.Log("GHamining hgared");
                GenerateNewTreasure(dickEntry);
            }
        }
    }

    void Update()
    {
        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            foreach (var treasure in treasureIndex) {
                if (treasure.state == TreasureState.DUG_UP)
                {
                    foreach (var dickpair in dick)
                    {
                        if (dickpair.Key == treasure.PlayerID)
                        {
                            GenerateNewTreasure(dickpair);
                        }
                    }
                }
            }
         /*   Debug.Log("Yo does this thing even work?");
            if (shouldSpawnTreasureForPlayer > 0)
            {
                foreach (var dickpair in dick)
                {
                    if (dickpair.Key == shouldSpawnTreasureForPlayer)
                    {
                        GenerateNewTreasure(dickpair);
                        shouldSpawnTreasureForPlayer = -1;
                        break;
                    }
                }
            }*/
        }
    }

    public PlayerController GetPlayerFromID(int id)
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is Trying to get playerID " + id);
        Debug.Log("This returned " + dick[id].photonView.Owner.NickName);
        return dick[id];
    }
    
    public void GenerateNewTreasure(KeyValuePair<int, PlayerController> dickEntry)
    {
        Vector3 randomPos = Random.insideUnitSphere * 40;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, 90, NavMesh.AllAreas);

        treasureIndex[dickEntry.Key - 1] = new TreasureData(hit.position, dickEntry.Key);
        treasureSpawn.SpawnTreasure(treasureIndex[dickEntry.Key - 1]);
        MapCaptureCam.QueueMapGenerate(treasureIndex[dickEntry.Key - 1]);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shouldSpawnTreasureForPlayer);
            activeMaps = 0;
            foreach (var treasure in treasureIndex)
            {
                if (treasure.PlayerID != 0)
                {
                    activeMaps++;
                }
            }
            stream.SendNext(activeMaps);
            foreach (var treasure in treasureIndex)
            {
                if (treasure.PlayerID != 0)
                {
                    stream.SendNext(treasure.TreasurePosition);
                    stream.SendNext(treasure.PlayerID);
                    stream.SendNext(treasure.state);

                }
            }
        }
        else
        {
            this.shouldSpawnTreasureForPlayer = (int)stream.ReceiveNext();

            activeMaps = (int)stream.ReceiveNext();
            for (int i = 0; i < activeMaps; i++)
            {
                TreasureData newData;
                newData.TreasurePosition = (Vector3)stream.ReceiveNext();
                newData.PlayerID = (int)stream.ReceiveNext();
                newData.state = (TreasureState)stream.ReceiveNext();

                TreasureData oldData = treasureIndex[newData.PlayerID - 1];

                if (!oldData.Equals(newData) && !newData.IsNull())
                {
                    Debug.Log("YOoooo QUEUEUEUEUEUEU " + treasureIndex[newData.PlayerID - 1].PlayerID + " " + treasureIndex[newData.PlayerID - 1].TreasurePosition);
                    treasureIndex[newData.PlayerID - 1] = newData;
                    if (MapCaptureCam == null)
                    {
                        Debug.LogError("MapCaptureCam is NULL!!!!");
                    }


                    
                    MapCaptureCam.QueueMapGenerate(this.treasureIndex[newData.PlayerID - 1]);
                }
            }
        }
    }
}
