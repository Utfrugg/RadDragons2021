using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public struct TreasureData
{
    public Vector3 TreasurePosition;
    public TreasureMap MapID;
    public RenderTexture mapTexture;
    public TreasureState state;
    public TreasureData(Vector3 TreasurePosition, TreasureMap MapID, RenderTexture mapTexture)
    {
        this.TreasurePosition = TreasurePosition;
        this.MapID = MapID;
        this.state = TreasureState.SPAWNED;
        this.mapTexture = mapTexture;
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

    float timeToBeDug = 1f;
    float timeDigging = 0;
    private void Start()
    {
        chest = transform.Find("chest");
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
                data.MapID.GenerateNewTreasure();
            }

            timeDigging += Time.deltaTime;
            chest.localPosition = new Vector3(0, Mathf.Lerp(-2, 0, Mathf.Min(1, (timeDigging/timeToBeDug))), 0);
        }
    }
}
