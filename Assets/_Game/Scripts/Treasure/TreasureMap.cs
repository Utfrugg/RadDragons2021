using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureMap : MonoBehaviour
{
    public Camera MapCaptureCam;

    private RenderTexture mapTexture;
    TreasureData CurrentTreasure;
    // Start is called before the first frame update

    void Start()
    {
        mapTexture = new RenderTexture(256,256,16);
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        newMaterial.SetTexture("_MainTex", mapTexture);
        GetComponent<Renderer>().sharedMaterial = newMaterial;


        GenerateNewTreasure(new Vector3 (Random.Range(-50, 50), 0, Random.Range(-50, 30)));
    }

    void GenerateNewTreasure(Vector3 position) {
        CurrentTreasure = new TreasureData(position, 0, mapTexture);
        MapCaptureCam.GetComponent<CreateMapTextures>().QueueMapGenerate(CurrentTreasure);
    }



    

    // Update is called once per frame
    void Update()
    {
        
    }
}
