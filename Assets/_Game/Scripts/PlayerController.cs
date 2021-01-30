using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenDivisions
{
    public static readonly Vector2 BottomLeft = new Vector2(0,0);
    public static readonly Vector2 BottomRight = new Vector2(0.5f, 0);
    public static readonly Vector2 TopLeft = new Vector2(0, 0.5f);
    public static readonly Vector2 TopRight = new Vector2(0.5f, 0.5f);
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    //TODO replace if possible
    public static GameObject LocalPlayerInstance;
    public static int TotalAmountOfPlayers = 0;

    private CharacterController ccontr;
    private PhotonView photonView;
    [SerializeField] private float speed = 10f;

    //Camera Values
    private Camera playerCam;
    private Quaternion cameraRotation = Quaternion.identity;
    private float xRotation = 0f; //Needed for the camera angle calculation

    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float gravity = -20f;

    [SerializeField] private GameObject cube;
    private bool isFiring = false;

    private bool grounded = false;
    private Vector3 velocity = Vector3.zero;

    void Awake()
    {
        playerCam = GetComponentInChildren<Camera>();
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        TotalAmountOfPlayers++;
        Vector2 screenDimension = new Vector2(0.5f, 0.5f);
        switch (TotalAmountOfPlayers)
        {
            case 1:
                playerCam.rect = new Rect(ScreenDivisions.TopLeft, screenDimension);
                break;
            case 2:
                playerCam.rect = new Rect(ScreenDivisions.TopRight, screenDimension);
                break;
            case 3:
                playerCam.rect = new Rect(ScreenDivisions.BottomLeft, screenDimension);
                break;
            case 4:
                playerCam.rect = new Rect(ScreenDivisions.BottomRight, screenDimension);
                break;
            default:
                Debug.LogError("Too Many Players!");
                break;
        }


        cube.SetActive(false);
        ccontr = GetComponent<CharacterController>();

        //Disabled for debug for now
        //Cursor.lockState = CursorLockMode.Locked;
        if (SceneManager.GetActiveScene().name == "LobbyRoom")
        {
            GameObject.FindObjectOfType<ReadyUpArea>().onAllPlayersReady.AddListener(LoadGameScene);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO Use this system to show the map. This way it shows up everywhere
        if (isFiring != cube.activeInHierarchy)
        {
            cube.SetActive(isFiring);
        }

        playerCam.transform.localRotation = cameraRotation;

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        UpdatePlayerPosition();
        UpdateCameraAngle();
        UpdatePlayerFire1();
    }

    private void UpdateCameraAngle()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraRotation = Quaternion.Euler(xRotation, 0f, 0f);
        this.transform.Rotate(Vector3.up * mouseX); //Rotate the player around the Y axis
    }

    private void UpdatePlayerPosition()
    {
        grounded = ccontr.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        ccontr.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        ccontr.Move(velocity * Time.deltaTime);
    }

    private void UpdatePlayerFire1()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!isFiring)
            {
                isFiring = true;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (isFiring)
            {
                isFiring = false;
            }
        }
    }

    private void LoadGameScene()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("SampleScene");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFiring);
        }
        else
        {
            this.isFiring = (bool) stream.ReceiveNext();
        }
    }
}
