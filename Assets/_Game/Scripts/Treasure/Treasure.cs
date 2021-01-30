using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TreasureData
{
    public Vector3 TreasurePosition;
    public int MapID;
    public RenderTexture mapTexture;
    public TreasureStatus status;
    public TreasureData(Vector3 TreasurePosition, int MapID, RenderTexture mapTexture)
    {
        this.TreasurePosition = TreasurePosition;
        this.MapID = MapID;
        this.status = TreasureStatus.Spawned;
        this.mapTexture = mapTexture;
    }
}

public enum TreasureStatus { 
    Spawned,
    Hidden,
    Found
}

public class Treasure : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
