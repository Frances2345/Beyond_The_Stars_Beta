using System.Collections.Generic;
using UnityEngine;

public class InfinitoFieldManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject asteroidTemplate;
    public Transform player;

    [Header("Zonas (mundo abierto)")]
    public float chunkSize = 80f;
    public int spawnRadiusChunks = 2;
    public int minAsteroidsPerChunk = 3;
    public int maxAsteroidsPerChunk = 6;
    public float chunkEdgePadding = 6f;
    public float spawnCheckInterval = 0.5f;
    public int maxActiveAsteroids = 140;

    [Header("Activacion por distancia")]
    public float activateDistance = 130f;
    public float deactivateDistance = 165f;
    public float destroyDistance = 400f;
    public float offscreenMargin = 8f;
    public float manageInterval = 0.5f;

    [Header("Enemigos")]
    public bool cullEnemies = true;
    public int maxEnemies = 45;
    public float despawnDistance = 120f;

    private readonly HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private readonly List<GameObject> spawnTemplates = new List<GameObject>();
    private readonly HashSet<GameObject> templateSet = new HashSet<GameObject>();
    private float spawnTimer;
    private float manageTimer;
    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
        CleanAsteroidIgnorers();
        AddAsteroidHitboxes();
        BuildTemplates();
    }

    void AddAsteroidHitboxes()
    {
        Asteroids[] existing = FindObjectsByType<Asteroids>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Asteroids a in existing)
        {
            if (a == null) continue;
            if (a.GetComponent<AsteroidHitbox>() == null) a.gameObject.AddComponent<AsteroidHitbox>();
        }
    }

    void CleanAsteroidIgnorers()
    {
        Asteroids[] existing = FindObjectsByType<Asteroids>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Asteroids a in existing)
        {
            if (a == null) continue;
            CollisionIgnorer ignorer = a.GetComponent<CollisionIgnorer>();
            if (ignorer != null) Destroy(ignorer);
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void BuildTemplates()
    {
        if (asteroidTemplate == null) return;

        Asteroids[] existing = FindObjectsByType<Asteroids>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        HashSet<AsteroidsSO> seen = new HashSet<AsteroidsSO>();

        foreach (Asteroids a in existing)
        {
            if (a == null || a.data == null) continue;
            if (!seen.Add(a.data)) continue;

            GameObject template = Instantiate(a.gameObject);
            template.name = "AsteroidSpawnTemplate_" + a.data.name;
            CollisionIgnorer ignorer = template.GetComponent<CollisionIgnorer>();
            if (ignorer != null) Destroy(ignorer);
            template.SetActive(false);
            spawnTemplates.Add(template);
            templateSet.Add(template);
        }

        if (spawnTemplates.Count == 0)
        {
            GameObject fallback = Instantiate(asteroidTemplate);
            fallback.name = "AsteroidSpawnTemplate";
            CollisionIgnorer fallbackIgnorer = fallback.GetComponent<CollisionIgnorer>();
            if (fallbackIgnorer != null) Destroy(fallbackIgnorer);
            fallback.SetActive(false);
            spawnTemplates.Add(fallback);
            templateSet.Add(fallback);
        }
    }

    void Update()
    {
        if (player == null) return;
        if (cam == null) cam = Camera.main;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnCheckInterval)
        {
            spawnTimer = 0f;
            GenerateChunks();
        }

        manageTimer += Time.deltaTime;
        if (manageTimer >= manageInterval)
        {
            manageTimer = 0f;
            ManageAsteroids();
            if (cullEnemies) CullEnemies();
        }
    }

    Vector2Int PlayerChunk()
    {
        return new Vector2Int(
            Mathf.FloorToInt(player.position.x / chunkSize),
            Mathf.FloorToInt(player.position.y / chunkSize));
    }

    Rect GetChunkRect(Vector2Int cell)
    {
        float centerX = (cell.x + 0.5f) * chunkSize;
        float centerY = (cell.y + 0.5f) * chunkSize;
        return new Rect(
            centerX - chunkSize * 0.5f,
            centerY - chunkSize * 0.5f,
            chunkSize,
            chunkSize);
    }

    Rect GetCameraRect()
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = cam.aspect * halfHeight;
        Vector3 c = cam.transform.position;
        float w = halfWidth + offscreenMargin;
        float h = halfHeight + offscreenMargin;
        return new Rect(c.x - w, c.y - h, w * 2f, h * 2f);
    }

    void GenerateChunks()
    {
        if (spawnTemplates.Count == 0 || cam == null) return;
        if (CountActiveAsteroids() >= maxActiveAsteroids) return;

        Vector2Int pc = PlayerChunk();
        Rect cameraRect = GetCameraRect();

        for (int x = -spawnRadiusChunks; x <= spawnRadiusChunks; x++)
        {
            for (int y = -spawnRadiusChunks; y <= spawnRadiusChunks; y++)
            {
                Vector2Int cell = new Vector2Int(pc.x + x, pc.y + y);
                if (generatedChunks.Contains(cell)) continue;

                Rect chunkRect = GetChunkRect(cell);
                if (chunkRect.Overlaps(cameraRect)) continue;

                GenerateChunk(cell, chunkRect);
            }
        }
    }

    void GenerateChunk(Vector2Int cell, Rect chunkRect)
    {
        int count = Random.Range(minAsteroidsPerChunk, maxAsteroidsPerChunk + 1);

        float minX = chunkRect.xMin + chunkEdgePadding;
        float maxX = chunkRect.xMax - chunkEdgePadding;
        float minY = chunkRect.yMin + chunkEdgePadding;
        float maxY = chunkRect.yMax - chunkEdgePadding;

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            GameObject template = spawnTemplates[Random.Range(0, spawnTemplates.Count)];
            GameObject asteroid = Instantiate(template, new Vector3(x, y, 0f), Quaternion.identity);
            asteroid.SetActive(true);
            if (asteroid.GetComponent<AsteroidHitbox>() == null) asteroid.AddComponent<AsteroidHitbox>();
        }

        generatedChunks.Add(cell);
    }

    int CountActiveAsteroids()
    {
        return FindObjectsByType<Asteroids>(FindObjectsSortMode.None).Length;
    }

    void ManageAsteroids()
    {
        Asteroids[] asteroids = FindObjectsByType<Asteroids>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Vector3 playerPos = player.position;

        foreach (Asteroids a in asteroids)
        {
            if (a == null) continue;
            if (templateSet.Contains(a.gameObject)) continue;

            float distance = Vector2.Distance(a.transform.position, playerPos);
            bool isActive = a.gameObject.activeSelf;

            if (destroyDistance > 0f && distance > destroyDistance)
            {
                Destroy(a.gameObject);
            }
            else if (isActive && distance > deactivateDistance)
            {
                a.gameObject.SetActive(false);
            }
            else if (!isActive && distance < activateDistance)
            {
                a.gameObject.SetActive(true);
            }
        }
    }

    void CullEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemies)
        {
            if (e == null) continue;
            if (Vector2.Distance(e.transform.position, player.position) > despawnDistance)
            {
                Destroy(e);
            }
        }
    }
}
