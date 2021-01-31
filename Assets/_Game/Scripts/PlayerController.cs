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

    [SerializeField] public GameObject map;
    private bool isLookingAtMap = false;
    private bool startDigging = false;

    private bool grounded = false;
    private Vector3 velocity = Vector3.zero;

    public TreasureCollider treasureColliderInRange = null;
    public int treasuresDugUp = 0;
    public TreasureData currentTreasure;

    private AnimStateController animStC;

    private MapManager mapManager;
    private CreateMapTextures MapCaptureCam;

    private Vector3 origMapPos;


#if DEBUG
    [SerializeField] private bool dontDoSplitScreen = true;
#endif

    void Awake()
    {
        animStC = GetComponentInChildren<AnimStateController>();
        playerCam = GetComponentInChildren<Camera>();
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            PlayerController.LocalPlayerInstance = this.gameObject;
        }

        DontDestroyOnLoad(this.gameObject);

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "GameplayScene")
            {
                mapManager = GameObject.FindObjectOfType<MapManager>();
                MapCaptureCam = GameObject.FindObjectOfType<CreateMapTextures>();
            }

            return;
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        TotalAmountOfPlayers++;
#if DEBUG
        if (!dontDoSplitScreen) { 
#endif
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

#if DEBUG
        
        }
#endif
        origMapPos = map.transform.position;
        map.transform.position = new Vector3(0, 1000, 0);
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
        if (isLookingAtMap != map.activeInHierarchy)
        {
            if(isLookingAtMap)
            {
                map.transform.position = origMapPos;
            }
            else
            {
                map.transform.position = new Vector3(0, 1000, 0);
            }
        }

        if (startDigging)
        {
            animStC.StartDiggingAnim();
            startDigging = false;
        }

        playerCam.transform.localRotation = cameraRotation;

        Vector2 animVelocity = new Vector2(velocity.x, velocity.z);
        animStC.UpdateMoveAnim(animVelocity.magnitude * 10);

        if (photonView.CreatorActorNr != photonView.ControllerActorNr)
            Debug.Log("CreateActorNR" + photonView.CreatorActorNr + "is NOT equal to ControllerCreatorNR" + photonView.ControllerActorNr);


        foreach (var bonk in PhotonNetwork.PlayerList) {
            if (bonk == PhotonNetwork.LocalPlayer)
            {
                Debug.Log("There is a localplayer with ID: " + bonk.ActorNumber);
            }
            if (bonk == PhotonNetwork.MasterClient)
            {
                Debug.Log("There is a masterplayer with ID: " + bonk.ActorNumber);
            }
            if (bonk != PhotonNetwork.LocalPlayer && bonk != PhotonNetwork.MasterClient)
                Debug.Log("There is another with ID: " + bonk.ActorNumber);
        }

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        UpdatePlayerPosition();
        UpdateCameraAngle();
        UpdatePlayerMap(); //Looking at maps
        UpdatePlayerDig(); //Digging up treasure
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

        velocity.x = move.x * speed * Time.deltaTime;
        velocity.z = move.z * speed * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        ccontr.Move(velocity * Time.deltaTime);
    }

    //Looking at Map
    private void UpdatePlayerMap()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (!isLookingAtMap)
            {
                isLookingAtMap = true;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (isLookingAtMap)
            {
                isLookingAtMap = false;
            }
        }
    }

    //Digging
    private void UpdatePlayerDig()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            startDigging = true;

            if (treasureColliderInRange != null)
            {
                treasureColliderInRange.DigUp();
                treasuresDugUp++;
            }
        }
    }

    private void LoadGameScene()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient && LocalPlayerInstance == this.gameObject)
        {
            if (PhotonNetwork.LevelLoadingProgress <= 0f)
            {
                PhotonNetwork.LoadLevel("GameplayScene");
            }
        }

        this.transform.position = new Vector3(this.transform.position.x, 10f, this.transform.position.z);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isLookingAtMap);
            stream.SendNext(currentTreasure.TreasurePosition);
            stream.SendNext(currentTreasure.PlayerID);
            stream.SendNext(currentTreasure.state);
        }
        else
        {
            this.isLookingAtMap = (bool) stream.ReceiveNext();

            TreasureData oldData = currentTreasure;
            TreasureData newData;
            newData.TreasurePosition = (Vector3)stream.ReceiveNext();
            newData.PlayerID = (int)stream.ReceiveNext();
            newData.state = (TreasureState)stream.ReceiveNext();

            if (!oldData.Equals(newData) && !newData.IsNull())
            {
                if (SceneManager.GetActiveScene().name != "LobbyRoom")
                {
                    currentTreasure = newData;
                    if (MapCaptureCam == null)
                    {
                        Debug.LogError("MapCaptureCam is NULL!!!!");
                    }


                    Debug.Log("YOoooo QUEUEUEUEUEUEU " + currentTreasure.PlayerID + " " + currentTreasure.TreasurePosition);
                    MapCaptureCam.QueueMapGenerate(this.currentTreasure);
                }
            }
        }
    }
}
