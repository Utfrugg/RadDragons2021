using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TreasureMap : MonoBehaviour
{
    public Camera MapCaptureCam;

    private RenderTexture mapTexture;
    TreasureData CurrentTreasure;
    public TreasureSpawner treasureSpawn;
    // Start is called before the first frame update

    void Start()
    {
        treasureSpawn = GameObject.FindObjectOfType<TreasureSpawner>();
        mapTexture = new RenderTexture(256,256,16);
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        newMaterial.SetTexture("_MainTex", mapTexture);
        GetComponent<Renderer>().sharedMaterial = newMaterial;

        Vector3 randomPos = Random.insideUnitSphere * 40; //0;

        NavMeshHit hit; // NavMesh Sampling Info Container

        // from randomPos find a nearest point on NavMesh surface in range of maxDistance
        NavMesh.SamplePosition(randomPos, out hit, 40, NavMesh.AllAreas);
        GenerateNewTreasure(hit.position);
    }

    void GenerateNewTreasure(Vector3 position) {
        treasureSpawn.SpawnTreasure(position);
        CurrentTreasure = new TreasureData(position, 0, mapTexture);
        MapCaptureCam.GetComponent<CreateMapTextures>().QueueMapGenerate(CurrentTreasure);


    }



    

    // Update is called once per frame
    void Update()
    {
        
    }
}
