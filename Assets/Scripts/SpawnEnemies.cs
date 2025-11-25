
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemies : MonoBehaviour
{
    public Transform jugador;
    public float distanciaExtra = 2f;

    public GameObject astroTrooperPrefab;
    public float astroTrooperCooldown = 5f;
    public float astroTrooperTimer = 0f;

    public GameObject AstroSpecialistPrefab;
    public float AstroSpecialistCooldown = 7f;
    public float AstroSpecialistTimer = 0f;

    public GameObject AstroStriderPrefab;
    public float AstroStriderCooldown = 10f;
    public float AstroStriderTimer = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        if(jugador == null)
        {
            return;
        }

        float dt = Time.deltaTime;

        astroTrooperTimer += dt;
        if(astroTrooperTimer >= astroTrooperCooldown)
        {
            SpawnEnemy(astroTrooperPrefab);
            astroTrooperTimer = 0f;
        }

        AstroSpecialistTimer += dt;
        if(AstroSpecialistTimer >=  AstroSpecialistCooldown)
        {
            SpawnEnemy(AstroSpecialistPrefab);
            AstroSpecialistTimer = 0f;
        }

        AstroStriderTimer += dt;
        if(AstroStriderTimer >= AstroStriderCooldown)
        {
            SpawnEnemy(AstroStriderPrefab);
            AstroStriderTimer = 0f;
        }
    }

    void SpawnEnemy(GameObject prefabToSpawn)
    {
        if(prefabToSpawn == null)
        {
            Debug.Log("No hay Prefab asignado en el codigo");
            return;
        }

        Camera cam = Camera.main;
        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.aspect * halfHeight;

        Vector3 camCenter = cam.transform.position;

        //Vector3 esquinaInferiorIzq = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        //Vector3 esquinaSuperiorDer = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));

        //float xMin = esquinaInferiorIzq.x - distanciaExtra;
        //float xMax = esquinaSuperiorDer.x - distanciaExtra;
        //float yMin = esquinaInferiorIzq.x - distanciaExtra;
        //float yMax = esquinaSuperiorDer.x - distanciaExtra;

        float xMin = camCenter.x - halfWidth - distanciaExtra;
        float xMax = camCenter.x + halfWidth + distanciaExtra;
        float yMin = camCenter.y - halfHeight - distanciaExtra;
        float yMax = camCenter.y + halfHeight + distanciaExtra;

        int borde = Random.Range(0, 4);
        Vector3 spawnPos = Vector3.zero;

        switch(borde)
        {
            case 0:
                spawnPos = new Vector3(xMin, Random.Range(yMin, yMax), 0);
                break;
            case 1:
                spawnPos = new Vector3(xMax, Random.Range(yMin, yMax), 0);
                break;
            case 2:
                spawnPos = new Vector3(Random.Range(xMin, xMax), yMin, 0);
                break;
            case 3:
                spawnPos = new Vector3(Random.Range(xMin, xMax), yMax, 0);
                break;
        }

        Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        Debug.Log("Ha llegado un " + prefabToSpawn.name + " .");

    }
}
