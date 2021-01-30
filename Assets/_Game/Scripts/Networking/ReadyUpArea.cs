using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReadyUpArea : MonoBehaviour
{
    public int playersReady = 0;
    public int playersNeeded = 2;
    public UnityEvent onAllPlayersReady = new UnityEvent();
    
    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            playersReady++;

            if (playersReady == playersNeeded)
            {
                //Load the next scene
                onAllPlayersReady.Invoke();
            }
        }
    }
}
