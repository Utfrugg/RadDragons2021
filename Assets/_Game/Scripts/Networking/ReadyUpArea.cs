using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class ReadyUpArea : MonoBehaviourPun
{
    public int playersReady = 0;
    public UnityEvent onAllPlayersReady = new UnityEvent();

    public float TimeToStart;
    private float countDown;
    private bool isLoading = false;
    private void Update()
    {
        int playersConnected = PhotonNetwork.CurrentRoom.PlayerCount;
        bool countingDown = (playersReady == playersConnected && playersConnected > 0);

        if (countingDown && !isLoading)
        {
            countDown += Time.deltaTime;
            if (countDown > TimeToStart) {
                isLoading = true;
                onAllPlayersReady.Invoke();
            }
        }
        else {
            countDown = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            playersReady++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            playersReady--;
        }
    }
}
