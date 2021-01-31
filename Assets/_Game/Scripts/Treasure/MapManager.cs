using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class MapManager : MonoBehaviourPunCallbacks, IPunObservable
{
    Dictionary<int, PlayerController> dick = new Dictionary<int, PlayerController>();
    
    private TreasureSpawner treasureSpawn;
    public int shouldSpawnTreasureForPlayer = -1;

    void Awake()
    {
        treasureSpawn = GameObject.FindObjectOfType<TreasureSpawner>();
        

        PlayerController[] allPlayersInScene = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayersInScene) 
        {
            dick.Add(player.photonView.ControllerActorNr, player);
        }

        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            foreach (var dickEntry in dick)
            {
                GenerateNewTreasure(dickEntry);
            }
        }
    }

    void Update()
    {
        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
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
            }
        }
    }

    public PlayerController GetPlayerFromID(int id)
    {
        return dick[id];
    }
    
    public void GenerateNewTreasure(KeyValuePair<int, PlayerController> dickEntry)
    {
        Vector3 randomPos = Random.insideUnitSphere * 40;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, 90, NavMesh.AllAreas);

        dickEntry.Value.currentTreasure = new TreasureData(hit.position, dickEntry.Key);
        treasureSpawn.SpawnTreasure(dickEntry.Value.currentTreasure);
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(shouldSpawnTreasureForPlayer);
        }
        else
        {
            this.shouldSpawnTreasureForPlayer = (int) stream.ReceiveNext();
        }
    }
}
