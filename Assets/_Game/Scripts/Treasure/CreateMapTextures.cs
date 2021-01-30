using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CreateMapTextures : MonoBehaviour
{
    public float CameraHeightOffset;
    private Queue<TreasureData> maps = new Queue<TreasureData>();


    Camera CameraComponent;
    // Start is called before the first frame update
    void Awake()
    {
        CameraComponent = GetComponent<Camera>();
        CameraComponent.enabled = false;
    }

    void Start()
    {
        RenderPipelineManager.beginFrameRendering += OnCameraPreRender;
        RenderPipelineManager.endFrameRendering += OnCameraPostRender;
    }


    public void QueueMapGenerate(TreasureData mapToGen){
        maps.Enqueue(mapToGen);
    }
    private void OnCameraPreRender(ScriptableRenderContext context, Camera[] camera)
    {
        if (maps.Count > 0)
        {
            CameraComponent.enabled = true;
            TreasureData currentMap = maps.Peek();
            this.transform.position = currentMap.TreasurePosition + new Vector3(0, CameraHeightOffset, 0);
            this.GetComponent<Camera>().targetTexture = currentMap.mapTexture;
        }
    }

    private void OnCameraPostRender(ScriptableRenderContext context, Camera[] camera)
    {
        if (CameraComponent.enabled) {
            CameraComponent.enabled = false;
            TreasureData currentMap = maps.Peek();
            currentMap.state = TreasureState.MAPGENERATED;
            maps.Dequeue();
        }

    }

    void OnDestroy()
    {
        RenderPipelineManager.beginFrameRendering -= OnCameraPreRender;
        RenderPipelineManager.endFrameRendering -= OnCameraPostRender;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
