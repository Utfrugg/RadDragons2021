using System;
using System.Collections;
using System.Collections.Generic;
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
}

public enum TreasureState
{
    SPAWNED,
    MAPGENERATED,
    DIGGING,
    DUG_UP
}

public class TreasureCollider : MonoBehaviour
{
    public TreasureData data;
    Transform chest;
    private MapManager mapManager;

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
    }
}
