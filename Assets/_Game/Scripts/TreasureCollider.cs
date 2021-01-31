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
    public int PlayerID;
    public TreasureState state;
    public TreasureData(Vector3 TreasurePosition, int playerID)
    {
        this.TreasurePosition = TreasurePosition;
        this.PlayerID = playerID;
        this.state = TreasureState.SPAWNED;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is TreasureData data))
            return false;

        return this.TreasurePosition == data.TreasurePosition &&
               this.PlayerID == data.PlayerID &&
               this.state == data.state;
    }

    public bool IsNull()
    {
        return this.PlayerID == 0;
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
    Transform chest;
    private MapManager mapManager;

    TreasureState oldState;

    float timeToBeDug = 1f;
    float timeDigging = 0;
    private void Start()
    {
        chest = transform.Find("chest");
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
    public void DigUp()
    {
        
        data.state = TreasureState.DIGGING;
        Debug.Log("Treasure Dug Up!");
    }

    private void Update()
    {
        if (data.state == TreasureState.DIGGING)
        {
            if (timeDigging >= timeToBeDug)
            {
                data.state = TreasureState.DUG_UP;
                //When a treasure has been dug up, generate a new treasure
                //Call this in mapManager
                mapManager.shouldSpawnTreasureForPlayer = data.PlayerID;
            }

            timeDigging += Time.deltaTime;
            chest.localPosition = new Vector3(0, Mathf.Lerp(-2, 0, Mathf.Min(1, (timeDigging/timeToBeDug))), 0);
        }
        if (oldState != data.state) {
            photonView.RequestOwnership();
            mapManager.treasureIndex[data.PlayerID - 1].state = data.state;
        }
        oldState = data.state;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Sup im gonna send some info");
            stream.SendNext(data.state);
            stream.SendNext(data.PlayerID);
        }
        else
        {
            Debug.Log("oh woops i just got some infoe hahahsdhads");
            data.state = (TreasureState) stream.ReceiveNext();
            data.PlayerID = (int)stream.ReceiveNext();
            oldState = data.state;
            mapManager.treasureIndex[data.PlayerID-1].state = data.state;
        }
    }
}
