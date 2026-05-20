using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyIndicator : MonoBehaviour
{
    [Header("Indicator Settings")]
    public float indicatorRadius = 3.5f;
    public int maxIndicators = 6;
    public Color indicatorColor = new Color(0.8f, 0.13f, 0f, 1f);
    public float arrowSize = 0.3f;

    private Camera mainCamera;
    private List<GameObject> indicators = new List<GameObject>();
    private List<EnemySloop> enemies = new List<EnemySloop>();

    void Start()
    {
        mainCamera = Camera.main;
        CreateIndicators();
    }

    void CreateIndicators()
    {
        for (int i = 0; i < maxIndicators; i++)
        {
            GameObject indicator = CreateArrow();
            indicator.SetActive(false);
            indicators.Add(indicator);
        }
    }

    GameObject CreateArrow()
    {
        GameObject obj = new GameObject("EnemyIndicator");
        obj.transform.parent = transform;

        // Create arrow mesh
        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        // Arrow pointing up — will be rotated to face enemy
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0.5f, 0f),      // tip
            new Vector3(-0.35f, -0.5f, 0f), // bottom left
            new Vector3(0.35f, -0.5f, 0f),  // bottom right
            new Vector3(-0.2f, -0.2f, 0f),  // inner bottom left
            new Vector3(0.2f, -0.2f, 0f),   // inner bottom right
            new Vector3(0f, 0.1f, 0f),      // inner tip
        };

        int[] triangles = new int[]
        {
            0, 2, 1,  // outer triangle
            3, 5, 4,  // inner triangle (cut out effect)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        // URP compatible material
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.color = indicatorColor;
        mr.material = mat;
        mr.sortingOrder = 20;

        obj.transform.localScale = Vector3.one * arrowSize;

        return obj;
    }

    void Update()
    {
        RefreshEnemyList();
        UpdateIndicators();
    }

    void RefreshEnemyList()
    {
        enemies.Clear();
        EnemySloop[] allEnemies = FindObjectsByType<EnemySloop>(FindObjectsSortMode.None);

        // Sort by distance and take closest maxIndicators
        enemies = allEnemies
            .OrderBy(e => Vector2.Distance(transform.position, e.transform.position))
            .Take(maxIndicators)
            .ToList();
    }

    void UpdateIndicators()
    {
        // Hide all first
        foreach (GameObject ind in indicators)
            ind.SetActive(false);

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null) continue;

            // Check if enemy is on screen
            Vector3 screenPos = mainCamera.WorldToViewportPoint(enemies[i].transform.position);
            bool onScreen = screenPos.x >= 0f && screenPos.x <= 1f &&
                            screenPos.y >= 0f && screenPos.y <= 1f &&
                            screenPos.z > 0f;

            if (onScreen)
            {
                indicators[i].SetActive(false);
                continue;
            }

            // Position on indicator circle around player
            Vector2 direction = (enemies[i].transform.position - transform.position).normalized;
            Vector2 indicatorPos = (Vector2)transform.position + direction * indicatorRadius;

            indicators[i].transform.position = new Vector3(indicatorPos.x, indicatorPos.y, 0f);

            // Rotate arrow to point toward enemy
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            indicators[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);

            indicators[i].SetActive(true);
        }
    }

    void OnDestroy()
    {
        foreach (GameObject ind in indicators)
        {
            if (ind != null)
                Destroy(ind);
        }
    }
}