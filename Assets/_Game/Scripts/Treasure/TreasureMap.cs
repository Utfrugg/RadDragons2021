using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public class TreasureMap : MonoBehaviour
{
    public RenderTexture mapTexture;

    public void InitMapInnit()
    {
        mapTexture = new RenderTexture(256, 256, 16);
        Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        newMaterial.SetTexture("_BaseMap", mapTexture);
        GetComponent<Renderer>().sharedMaterial = newMaterial;
    }


    

    // Update is called once per frame
    void Update()
    {
        
    }
}
