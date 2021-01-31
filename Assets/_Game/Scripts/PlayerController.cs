﻿using System.Collections;
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

    public bool amIloaded;
    public bool[] playersLoaded = { false, false, false, false };

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
                mapManager = GameObject.FindObjectOfType<MapManager>();
                MapCaptureCam = GameObject.FindObjectOfType<CreateMapTextures>();
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
            rend.sharedMaterial = playerMaterials[TotalAmountOfPlayers - 1];
        }

        digParticles.Stop();

#if DEBUG

        }
#endif
        
        ccontr = GetComponent<CharacterController>();

        //Disabled for debug for now
        //Cursor.lockState = CursorLockMode.Locked;
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

        if (SceneManager.GetActiveScene().name == "IslandScene")
        {
            if (!mapManager.everybodyloaded && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                bool stillGood = true;
                int goodCount = 4;
                foreach (var goodbool in playersLoaded)
                {
                    if (stillGood == false || goodbool == false)
                    {
                        stillGood = false;
                        goodCount--;
                    }
                }

                Debug.Log(goodCount + "People loaded into the map");
                mapManager.everybodyloaded = stillGood;
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
            stream.SendNext(photonView.ControllerActorNr);
            stream.SendNext(amIloaded);

            stream.SendNext((mapManager == null) ? false : mapManager.everybodyloaded);
        }
        else
        {
            this.isLookingAtMap = (bool) stream.ReceiveNext();
            int regID = (int)stream.ReceiveNext();
            playersLoaded[regID] = (bool)stream.ReceiveNext();
            bool loadyuuh = (bool)stream.ReceiveNext();


            if (!PhotonNetwork.LocalPlayer.IsMasterClient && loadyuuh) {
                mapManager.everybodyloaded = loadyuuh;
            }
        }
    }
}
