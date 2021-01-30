using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public enum TreasureState
{
    HIDDEN,
    DIGGING,
    DUG_UP
}

public class TreasureCollider : MonoBehaviour
{
    public TreasureState state = TreasureState.HIDDEN;

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.treasureColliderInRange = this;
        }
    }

    public void DigUp()
    {
        state = TreasureState.DIGGING;
        Debug.Log("Treasure Dug Up!");
    }
}
