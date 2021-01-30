using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class TreasureMap : MonoBehaviour
{


    private RenderTexture mapTexture;
    TreasureData CurrentTreasure;
    private CreateMapTextures MapCaptureCam;
    private TreasureSpawner treasureSpawn;
    // Start is called before the first frame update

    void Start()
    {
        treasureSpawn = GameObject.FindObjectOfType<TreasureSpawner>();
        MapCaptureCam = GameObject.FindObjectOfType<CreateMapTextures>();
        mapTexture = new RenderTexture(256,256,16);
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
        newMaterial.SetTexture("_MainTex", mapTexture);
        GetComponent<Renderer>().sharedMaterial = newMaterial;

        if (SceneManager.GetActiveScene().name != "LobbyRoom")
            GenerateNewTreasure();
    }

    public void GenerateNewTreasure() {
        Vector3 randomPos = Random.insideUnitSphere * 40;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, 90, NavMesh.AllAreas);

        CurrentTreasure = new TreasureData(hit.position, this, mapTexture);
        treasureSpawn.SpawnTreasure(CurrentTreasure);
        MapCaptureCam.QueueMapGenerate(CurrentTreasure);
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
