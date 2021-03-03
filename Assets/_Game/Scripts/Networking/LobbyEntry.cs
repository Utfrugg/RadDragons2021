using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntry : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    public string roomName = "Epic Room";

    // Start is called before the first frame update
    public void Init(string rname)
    {
        roomName = rname;
        label.text = rname;
        button.onClick.AddListener(ButtonClicked);
        
    }

    private void ButtonClicked()
    {
        GameObject.FindObjectOfType<Launcher>().JoinRoom(roomName);
    }
}
