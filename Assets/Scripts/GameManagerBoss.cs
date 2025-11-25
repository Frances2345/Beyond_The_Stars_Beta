using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform[] spawnPoints;

    public GameObject astroTrooperPrefab;
    public float astroTrooperCooldown = 5f;
    private float astroTrooperTimer = 0f;

    public GameObject astroSpecialistPrefab;
    public float astroSpecialistCooldown = 7f;
    private float astroSpecialistTimer = 0f;

    public GameObject astroStriderPrefab;
    public float astroStriderCooldown = 10f;
    private float astroStriderTimer = 0f;

    public GameObject patrolEnemyPrefab;
    public float patrolEnemyCooldown = 8f;
    private float patrolEnemyTimer = 0f;

    void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("El arreglo de SpawnPoints está vacío o nulo. Asigna tus Spawners en el Inspector.");
            enabled = false;
        }
    }

    void Update()
    {
        if (spawnPoints.Length == 0) return;

        float dt = Time.deltaTime;

        astroTrooperTimer += dt;
        if (astroTrooperTimer >= astroTrooperCooldown)
        {
            SpawnEnemy(astroTrooperPrefab);
            astroTrooperTimer = 0f;
        }

        astroSpecialistTimer += dt;
        if (astroSpecialistTimer >= astroSpecialistCooldown)
        {
            SpawnEnemy(astroSpecialistPrefab);
            astroSpecialistTimer = 0f;
        }

        astroStriderTimer += dt;
        if (astroStriderTimer >= astroStriderCooldown)
        {
            SpawnEnemy(astroStriderPrefab);
            astroStriderTimer = 0f;
        }

        patrolEnemyTimer += dt;
        if (patrolEnemyTimer >= patrolEnemyCooldown)
        {
            SpawnEnemy(patrolEnemyPrefab);
            patrolEnemyTimer = 0f;
        }
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || spawnPoints.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform selectedSpawner = spawnPoints[randomIndex];

        Instantiate(enemyPrefab, selectedSpawner.position, Quaternion.identity);
    }
}