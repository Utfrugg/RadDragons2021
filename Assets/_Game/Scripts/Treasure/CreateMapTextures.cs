using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateMapTextures : MonoBehaviour
{
    public float CameraHeightOffset;
    private Queue<TreasureData> maps = new Queue<TreasureData>();
    private MapManager mapManager;

    Camera CameraComponent;
    // Start is called before the first frame update
    void Awake()
    {
        CameraComponent = GetComponent<Camera>();
        CameraComponent.enabled = false;

        mapManager = GameObject.FindObjectOfType<MapManager>();
    }

    void Start()
    {
        RenderPipelineManager.endCameraRendering += OnCameraPostRender;
    }


    public void QueueMapGenerate(TreasureData mapToGen){
        Debug.Log("<color=red>Just queued a Map for PlayerID: " + mapToGen.PlayerID + " at position: " + mapToGen.TreasurePosition + "</color>");
        maps.Enqueue(mapToGen);
    }

    private void OnCameraPostRender(ScriptableRenderContext context, Camera camera)
    {
            if (CameraComponent.enabled)
            {
                CameraComponent.enabled = false;
            }
            if (maps.Count > 0)
            {
                TreasureData currentMap = maps.Peek();
                Debug.Log("<color=blue>Just generated a Map for PlayerID: " + currentMap.PlayerID + " at position: " + currentMap.TreasurePosition + "</color>");
                CameraComponent.enabled = true;
                this.transform.position = currentMap.TreasurePosition + new Vector3(0, CameraHeightOffset, 0);
                this.GetComponent<Camera>().targetTexture = mapManager.GetPlayerFromID(currentMap.PlayerID).map.GetComponentInChildren<TreasureMap>().mapTexture;
            if (currentMap.state == mapManager.treasureIndex[currentMap.PlayerID-1].state) {
                maps.Enqueue(currentMap);
            }
                maps.Dequeue();
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
