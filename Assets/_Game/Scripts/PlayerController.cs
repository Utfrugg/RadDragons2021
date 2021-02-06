using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
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

    public int playersNeeded = 2;
    public bool amIloaded;
    public static bool[] playersLoaded = { false, false, false, false };

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

    public Material[] playerMaterials;

    public ParticleSystem runParticles;
    public ParticleSystem digParticles;
    [SerializeField] public GameObject shovel;

    public int score = 0;
    [SerializeField] private int stolenScore = 2;
    [SerializeField] private int normalScore = 1;

    [SerializeField] private TMP_Text playerNameText;


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
            if (scene.name == "IslandScene")
            {
                score = 0;
                //Needed for resetting
                TotalAmountOfPlayers = 0;

                Cursor.lockState = CursorLockMode.Locked;
                mapManager = GameObject.FindObjectOfType<MapManager>();
                MapCaptureCam = GameObject.FindObjectOfType<CreateMapTextures>();

                amIloaded = true;
                if (photonView.Controller.IsMasterClient)
                {
                    PlayerController.playersLoaded[photonView.ControllerActorNr - 1] = true;
                }

                GameObject.FindObjectOfType<ProgressTracker>().PlayerInnit(this);
            }

            return;
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        map.GetComponentInChildren<TreasureMap>().InitMapInnit();
        TotalAmountOfPlayers++;
#if DEBUG
        if (!dontDoSplitScreen) { 
#endif

            
        Vector2 screenDimension = new Vector2(0.5f, 0.5f);

        var renderers = transform.GetChild(0).gameObject.GetComponentsInChildren<Renderer>();

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

        foreach (var rend in renderers)
        {
            rend.sharedMaterial = playerMaterials[photonView.ControllerActorNr - 1];
        }

        playerNameText.text = photonView.Controller.NickName;
        

        digParticles.Stop();

#if DEBUG

        }
#endif
        
        ccontr = GetComponent<CharacterController>();

        //Disabled for debug for now
        Cursor.lockState = CursorLockMode.Locked;
        if (SceneManager.GetActiveScene().name == "LobbyRoom")
        {
            GameObject.FindObjectOfType<ReadyUpArea>().onAllPlayersReady.AddListener(LoadGameScene);
        }
    }

    IEnumerator StopDigParticles()
    {
        yield return new WaitForSeconds(4f);
        digParticles.Stop();
        shovel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO Use this system to show the map. This way it shows up everywhere
        if ((isLookingAtMap && map.layer == LayerMask.NameToLayer("OnlyOnMap")) 
            || (!isLookingAtMap && map.layer == LayerMask.NameToLayer("Default")))
        {
            if(isLookingAtMap)
            {
                map.layer = LayerMask.NameToLayer("Default");
                for (int i = 0; i < map.transform.childCount; i++)
                {
                    map.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Default");
                }
                
            }
            else
            {
                map.layer = LayerMask.NameToLayer("OnlyOnMap");
                for (int i = 0; i < map.transform.childCount; i++)
                {
                    map.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("OnlyOnMap");
                }
            }
        }

        if (startDigging)
        {
            animStC.StartDiggingAnim();
            digParticles.Play();
            shovel.SetActive(true);
            StartCoroutine(StopDigParticles());
            startDigging = false;
        }

        playerCam.transform.localRotation = cameraRotation;

        Vector2 animVelocity = new Vector2(velocity.x, velocity.z);
        animStC.UpdateMoveAnim(animVelocity.magnitude * 10);
        if (animVelocity.magnitude > 0.0f)
        {
            runParticles.Play();
        }
        else
        {
            runParticles.Stop();
        }


        //foreach (var bonk in PhotonNetwork.PlayerList) 
        //{
        //    if (bonk == PhotonNetwork.LocalPlayer)
        //    {
        //        Debug.Log("There is a localplayer with ID: " + bonk.ActorNumber);
        //    }
        //    if (bonk == PhotonNetwork.MasterClient)
        //    {
        //        Debug.Log("There is a masterplayer with ID: " + bonk.ActorNumber);
        //    }
        //    if (bonk != PhotonNetwork.LocalPlayer && bonk != PhotonNetwork.MasterClient)
        //        Debug.Log("There is another with ID: " + bonk.ActorNumber);
        //}

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        
        }

        if (SceneManager.GetActiveScene().name == "IslandScene")
        {
            if (!mapManager.everybodyloaded && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                
                int goodCount = 0;
                foreach (var goodbool in playersLoaded)
                {
                    if (goodbool == false)
                    {
                        break;
                    }
                    goodCount++;
                }

                Debug.Log(goodCount + "/" + playersNeeded + "People loaded into the map");
                if (goodCount >= playersNeeded) 
                {
                    mapManager.everybodyloaded = true;
                    mapManager.SecondInitInnit();
                }
            }
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
                if (treasureColliderInRange.data.OwningPlayerID == photonView.ControllerActorNr)
                {
                    score += normalScore;
                }
                else
                {
                    score += stolenScore;
                }
                treasureColliderInRange.DigUp(photonView.ControllerActorNr);
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
                PhotonNetwork.LoadLevel("IslandScene");
            }
        }

        this.transform.position = new Vector3(this.transform.position.x, 10f, this.transform.position.z);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isLookingAtMap);
            stream.SendNext(startDigging);
            stream.SendNext(score);

            stream.SendNext(photonView.ControllerActorNr);
            stream.SendNext(amIloaded);

            stream.SendNext((mapManager == null) ? false : mapManager.everybodyloaded);
        }
        else
        {
            this.isLookingAtMap = (bool) stream.ReceiveNext();
            startDigging = (bool) stream.ReceiveNext();
            score = (int) stream.ReceiveNext();

            int regID = (int)stream.ReceiveNext();
            playersLoaded[regID - 1] = (bool)stream.ReceiveNext();
            bool loadyuuh = (bool)stream.ReceiveNext();

            if (SceneManager.GetActiveScene().name == "IslandScene" && photonView.Controller.IsMasterClient)
            {
                Debug.Log("Checking whether everybody is loaded is being sent over");
                if (loadyuuh && !mapManager.everybodyloaded)
                {
                    Debug.Log("<color=pink>Hey uuuh I heard everybody lloaded, we going??? gamignng???/</color>");
                    mapManager.everybodyloaded = loadyuuh;
                    mapManager.SecondInitInnit();
                }
            }
        }
    }
}
