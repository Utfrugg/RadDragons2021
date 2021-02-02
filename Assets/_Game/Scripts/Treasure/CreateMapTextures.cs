using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateMapTextures : MonoBehaviour
{
    public float CameraHeightOffset;
    private Queue<TreasureData> maps = new Queue<TreasureData>();
    private Queue<int> playerIDs = new Queue<int>();
    private MapManager mapManager;

    public RenderTexture tempTex;

    Camera CameraComponent;
    // Start is called before the first frame update
    void Awake()
    {
        tempTex = new RenderTexture(256, 256, 16);
        CameraComponent = GetComponent<Camera>();
        //CameraComponent.enabled = false;

        mapManager = GameObject.FindObjectOfType<MapManager>();
    }

    void Start()
    {
        RenderPipelineManager.endCameraRendering += OnCameraPostRender;
    }

    public void QueueMapGenerate(TreasureData mapToGen){
        Debug.Log("<color=red>Just queued a Map for PlayerID: " + mapToGen.PlayerID + " at position: " + mapToGen.TreasurePosition + "</color>");
        if (!playerIDs.Contains(mapToGen.PlayerID))
        {
            playerIDs.Enqueue(mapToGen.PlayerID);
            maps.Enqueue(mapToGen);
        }
    }

    private void OnCameraPostRender(ScriptableRenderContext context, Camera camera)
    {
        Debug.Log("<color=yellow>Is Camera with name " + camera.name + "the same as </color>" + CameraComponent.name);
        if (camera.name == CameraComponent.name)
        {
            CameraComponent.targetTexture = tempTex;
            Debug.Log("<color=purple> YESYEYSYYSYEYSYSYSYEYSYS </color>");
            if (maps.Count > 0)
            {
                TreasureData currentMap = maps.Peek();
                Debug.Log("<color=blue>Just generated a Map for PlayerID: " + currentMap.PlayerID + " at position: " + currentMap.TreasurePosition + "</color>");
                this.transform.position = currentMap.TreasurePosition + new Vector3(0, CameraHeightOffset, 0);
                this.GetComponent<Camera>().targetTexture = mapManager.GetPlayerFromID(currentMap.PlayerID).map.GetComponentInChildren<TreasureMap>().mapTexture;
                playerIDs.Dequeue();
                maps.Dequeue();
            }
        }
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnCameraPostRender;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
