using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

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

    private Camera playerCam;
    private CharacterController ccontr;
    private PhotonView photonView;
    private float speed = 10f;

    [SerializeField] private GameObject cube;
    private bool isFiring = false;

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

#if UNITY_5_4_OR_NEWER
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        //TODO Use this system to show the map. This way it shows up everywhere
        if (isFiring != cube.activeInHierarchy)
        {
            cube.SetActive(isFiring);
        }

        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        ccontr.Move(move * speed * Time.deltaTime);

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

#if UNITY_5_4_OR_NEWER
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
    {
        this.CalledOnLevelWasLoaded(scene.buildIndex);
    }
#endif

#if !UNITY_5_4_OR_NEWER
    void OnLevelWasLoaded(int level)
    {
        this.CalledOnLevelWasLoaded(level);
    }
#endif

    void CalledOnLevelWasLoaded(int level)
    {
        if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
        {
            transform.position = new Vector3(0f, 5f, 0f);
        }
    }

#if UNITY_5_4_OR_NEWER
    public override void OnDisable()
    {
        base.OnDisable();
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
#endif
}
