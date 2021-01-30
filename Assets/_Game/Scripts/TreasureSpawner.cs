using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TreasureSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject treasurePrefab;

    private Dictionary<Player, TreasureCollider> treasures = new Dictionary<Player, TreasureCollider>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            //Spawn the treasures somewhere
            //treasures.Add(player, SpawnTreasure());
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var treasure in treasures)
        {
            switch (treasure.Value.data.state)
            {
                case TreasureState.MAPGENERATED:
                    break;
                case TreasureState.DIGGING:
                    
                    break;
                case TreasureState.DUG_UP:
                    //Despawn the Treasure

                    //Spawn a new Treasure
                   // treasures[treasure.Key] = SpawnTreasure();
                    break;
            }
        }
    }

    public TreasureCollider SpawnTreasure(TreasureData data)
    {
        TreasureCollider newTreasure = PhotonNetwork.Instantiate("Treasure", data.TreasurePosition, Quaternion.identity).GetComponent<TreasureCollider>();
        newTreasure.data = data;
        return newTreasure;
    }
}
