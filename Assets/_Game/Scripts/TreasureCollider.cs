using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public struct TreasureData
{
    public Vector3 TreasurePosition;
    public int OwningPlayerID;
    public int DiggingPlayerID;
    public TreasureState state;
    public TreasureData(Vector3 TreasurePosition, int playerID)
    {
        this.TreasurePosition = TreasurePosition;
        this.OwningPlayerID = playerID;
        this.DiggingPlayerID = -1;
        this.state = TreasureState.SPAWNED;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is TreasureData data))
            return false;

        return this.TreasurePosition == data.TreasurePosition &&
               this.OwningPlayerID == data.OwningPlayerID &&
               this.DiggingPlayerID == data.DiggingPlayerID &&
               this.state == data.state;
    }

    public bool IsNull()
    {
        return this.OwningPlayerID == 0;
    }
}

public enum TreasureState
{
    SPAWNED,
    MAPGENERATED,
    DIGGING,
    DUG_UP
}

public class TreasureCollider : MonoBehaviourPunCallbacks, IPunObservable
{
    public TreasureData data;
    Transform BigX;
    Transform chest;
    private MapManager mapManager;

    TreasureState oldState;

    float timeToBeDug = 1f;
    float timeDigging = 0;
    private void Start()
    {
        oldState = data.state;
        chest = transform.Find("chest");
        BigX = transform.Find("CrossBonePlane");
        mapManager = GameObject.FindObjectOfType<MapManager>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            player.treasureColliderInRange = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player)) 
            player.treasureColliderInRange = null;
    }
    public void DigUp(int diggingPlayer)
    {
        if (data.state == TreasureState.SPAWNED) {
            data.DiggingPlayerID = diggingPlayer;
            data.state = TreasureState.DIGGING;
            Debug.Log("Treasure Dug Up!");

            photonView.RequestOwnership();
        }
    }

    private void Update()
    {
        if (data.state == TreasureState.DIGGING && data.DiggingPlayerID == photonView.ControllerActorNr)
        {
            if (timeDigging >= timeToBeDug)
            {
                data.state = TreasureState.DUG_UP;
                BigX.gameObject.SetActive(false);
                mapManager.shouldSpawnTreasureForPlayer = data.OwningPlayerID;
            }

            timeDigging += Time.deltaTime;
            chest.localPosition = new Vector3(0, Mathf.Lerp(-2, 0, Mathf.Min(1, (timeDigging/timeToBeDug))), 0);
        }
        if (oldState != data.state) {
            photonView.RequestOwnership();
            
            mapManager.treasureIndex[data.OwningPlayerID - 1].state = data.state;
        }
        oldState = data.state;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Sup im gonna send some info");
            stream.SendNext(data.state);
            stream.SendNext(data.OwningPlayerID);
            stream.SendNext(data.DiggingPlayerID);
        }
        else
        {
            data.state = (TreasureState)stream.ReceiveNext();
            data.OwningPlayerID = (int)stream.ReceiveNext();
            data.DiggingPlayerID = (int)stream.ReceiveNext();
            if ((mapManager == null) ? false : mapManager.everybodyloaded)
            {
                oldState = data.state;
                Debug.Log("YO CAN YOU BELIEVE IT SOMEBODY JUST DUG UP A TREASURE AT " +  data.TreasurePosition);
                if (data.state != TreasureState.SPAWNED)
                {
                    BigX.gameObject.SetActive(false);
                }

                if (mapManager.gameObject.activeInHierarchy)
                {
                    if (mapManager.treasureIndex[data.OwningPlayerID - 1].TreasurePosition == data.TreasurePosition)
                    {
                        mapManager.treasureIndex[data.OwningPlayerID - 1].state = data.state;
                        mapManager.treasureIndex[data.OwningPlayerID - 1].DiggingPlayerID = data.DiggingPlayerID;
                    }
                }
            }
        } 
    }

    public void IOnPhotonViewOwnerChange() {
        Debug.Log("<color=yellow>Yarr, treasure at" + data.TreasurePosition + " has been taken ownership of by player " + photonView.OwnerActorNr + " as it state changed from " + oldState + " to " + data.state + "</color>");
    }
}
