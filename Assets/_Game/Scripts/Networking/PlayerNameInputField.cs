using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    private static string playerNamePrefKey = "PlayerName";
    

    // Start is called before the first frame update
    void Start()
    {
        string defaultName = "";
        TMP_InputField inputField = GetComponent<TMP_InputField>();

        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            defaultName = PlayerPrefs.GetString(playerNamePrefKey);
            inputField.text = defaultName;
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = defaultName;
        }
    }

    public void SetPlayerName(string pname)
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = pname + " ";
        }
        PlayerPrefs.SetString(playerNamePrefKey, pname);
    }
}
