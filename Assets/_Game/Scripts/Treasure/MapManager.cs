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

    public float playersNeeded = 2;
    public TreasureData[] treasureIndex;

    private TreasureSpawner treasureSpawn;
    public int shouldSpawnTreasureForPlayer = -1;

    public bool initinnit = false;
    public bool everybodyloaded;
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

        initinnit = true;
        //
    }


    public void SecondInitInnit() {
        PlayerController[] allPlayersInScene = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in allPlayersInScene)
        {
            dick.Add(player.photonView.ControllerActorNr, player);
            Debug.Log("<color=yellow>Playername: " + player.photonView.Controller.NickName + " id: " + player.photonView.ControllerActorNr + "</color>");
        }

        //move this over to mapgen init
        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            foreach (var dickEntry in dick)
            {
                Debug.Log("GHamining hgared");
                GenerateNewTreasure(dickEntry.Key);
            }
        }

    }

    void Update()
    {
        if (PhotonNetwork.LocalPlayer.Equals(PhotonNetwork.MasterClient))
        {
            for (int i = 0; i < 4; i++)
            {
                 if (treasureIndex[i].state == TreasureState.DUG_UP)
                 {
                    Debug.Log("Okay it seems the treasure for ID " + (i+1) + "got dug up");
                    GenerateNewTreasure(i+1);
                    int a = 3;
                 }
            }
        }
    }

    public PlayerController GetPlayerFromID(int id)
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is Trying to get playerID " + id);
        Debug.Log("This returned " + dick[id].photonView.Owner.NickName);
        return dick[id];
    }
    
    public void GenerateNewTreasure(int dickKey)
    {
        Vector3 randomPos = Random.insideUnitSphere * 70 + new Vector3(-20, 10, -20);

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, 90, NavMesh.AllAreas);

        treasureIndex[dickKey - 1] = new TreasureData(hit.position, dickKey);
        treasureSpawn.SpawnTreasure(treasureIndex[dickKey - 1]);
        MapCaptureCam.QueueMapGenerate(treasureIndex[dickKey - 1]);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.Log("mapmanager on photonserlieliza view please??");
        if (stream.IsWriting)
            {
                Debug.Log("starting writing uuuufhhfhfhfhfhf");
                stream.SendNext(shouldSpawnTreasureForPlayer);
                activeMaps = 0;
                foreach (var treasure in treasureIndex)
                {
                    if (treasure.OwningPlayerID != 0)
                    {
                        activeMaps++;
                    }
                }
                Debug.Log("activemaps " + activeMaps);

                stream.SendNext(activeMaps);
                foreach (var treasure in treasureIndex)
                {
                    if (treasure.OwningPlayerID != 0)
                    {
                        stream.SendNext(treasure.TreasurePosition);
                        stream.SendNext(treasure.OwningPlayerID);
                        stream.SendNext(treasure.state);
                        Debug.Log("Just sent some info boutta treassure fooor plaayer" + treasure.OwningPlayerID + " with staaate: " + treasure.state + " at position " + treasure.TreasurePosition);
                    }
                }
            }
            else
            {

                this.shouldSpawnTreasureForPlayer = (int)stream.ReceiveNext();

                activeMaps = (int)stream.ReceiveNext();
            for (int i = 0; i < activeMaps; i++)
            {
                TreasureData newData = new TreasureData();
                newData.TreasurePosition = (Vector3)stream.ReceiveNext();
                newData.OwningPlayerID = (int)stream.ReceiveNext();
                newData.state = (TreasureState)stream.ReceiveNext();
                if (everybodyloaded)
                {
                    
                    TreasureData oldData = treasureIndex[newData.OwningPlayerID - 1];
                    Debug.Log("Just got some info boutta treassure fooor plaayer" + newData.OwningPlayerID + " with staaate: " + newData.state + " at position " + newData.TreasurePosition);
                    if (!oldData.Equals(newData) && !newData.IsNull())
                    {
                        Debug.Log("YOoooo QUEUEUEUEUEUEU " + treasureIndex[newData.OwningPlayerID - 1].OwningPlayerID + " " + treasureIndex[newData.OwningPlayerID - 1].TreasurePosition);
                        treasureIndex[newData.OwningPlayerID - 1] = newData;
                        if (MapCaptureCam == null)
                        {
                            Debug.LogError("MapCaptureCam is NULL!!!!");
                        }
                        MapCaptureCam.QueueMapGenerate(this.treasureIndex[newData.OwningPlayerID - 1]);
                    }
                }
            }
        }
    }
}
