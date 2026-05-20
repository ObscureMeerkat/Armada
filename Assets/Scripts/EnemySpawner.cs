using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemySloopPrefab;
    public float mapWidth = 130f;
    public float mapHeight = 130f;
    public float safeZoneRadius = 20f;

    [Header("Escalation Settings")]
    public int initialEnemyCount = 10;
    public int enemyCountIncrement = 5;
    public float spawnThresholdPercent = 0.3f;
    public float reinforcementCountdown = 5f;

    private int currentWaveSize;
    private int nextWaveSize;
    private bool countingDown = false;
    private float countdownTimer = 0f;
    private Transform player;

    public static EnemySpawner Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentWaveSize = initialEnemyCount;
        nextWaveSize = initialEnemyCount + enemyCountIncrement;
        SpawnEnemies(currentWaveSize);
    }

    void Update()
    {
        if (countingDown)
        {
            countdownTimer -= Time.deltaTime;

            if (HUDManager.Instance != null)
                HUDManager.Instance.SetReinforcementTimer(
                    Mathf.CeilToInt(countdownTimer));

            if (countdownTimer <= 0f)
            {
                countingDown = false;
                if (HUDManager.Instance != null)
                    HUDManager.Instance.HideReinforcementTimer();

                SpawnEnemies(nextWaveSize);
                currentWaveSize = nextWaveSize;
                nextWaveSize += enemyCountIncrement;
            }
            return;
        }

        // Check if threshold reached
        int remaining = GetEnemyCount();

        if (HUDManager.Instance != null)
            HUDManager.Instance.SetEnemyCount(remaining);

        int threshold = Mathf.Max(1,
            Mathf.RoundToInt(currentWaveSize * spawnThresholdPercent));

        if (remaining <= threshold && remaining > 0)
        {
            countingDown = true;
            countdownTimer = reinforcementCountdown;
        }
    }

    void SpawnEnemies(int count)
    {
        int placed = 0;
        int attempts = 0;

        while (placed < count && attempts < 300)
        {
            attempts++;

            float x = Random.Range(-mapWidth / 2, mapWidth / 2);
            float y = Random.Range(-mapHeight / 2, mapHeight / 2);
            Vector2 pos = new Vector2(x, y);

            // Stay away from player
            if (player != null &&
                Vector2.Distance(pos, player.position) < safeZoneRadius)
                continue;

            Instantiate(enemySloopPrefab, pos,
                       Quaternion.Euler(0, 0, Random.Range(0f, 360f)));
            placed++;
        }
    }

    int GetEnemyCount()
    {
        return FindObjectsByType<EnemySloop>(FindObjectsSortMode.None).Length;
    }
}