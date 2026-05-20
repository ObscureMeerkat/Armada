using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int seed = 42;
    public float mapWidth = 140f;
    public float mapHeight = 140f;
    public float safeZoneRadius = 12f;

    [Header("Island Settings")]
    public int islandCount = 12;
    public float islandMinSize = 1.5f;
    public float islandMaxSize = 3.5f;
    public float minIslandDistance = 15f;

    [Header("Shallow Settings")]
    public int shallowCount = 15;
    public float shallowMinSize = 2f;
    public float shallowMaxSize = 6f;

    [Header("Rock Settings")]
    public int rockGroupCount = 10;
    public int rocksPerGroup = 3;
    public float rockMinSize = 0.3f;
    public float rockMaxSize = 0.8f;

    [Header("Colors")]
    public Color islandColor = new Color(0.93f, 0.87f, 0.65f, 1f);
    public Color shallowColor = new Color(0.7f, 0.9f, 1f, 0.5f);
    public Color rockColor = new Color(0.25f, 0.25f, 0.28f, 1f);
    public Color foamColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Sorting Orders")]
    public int shallowOrder = 1;
    public int islandOrder = 2;
    public int foamOrder = 3;
    public int rockOrder = 4;

    private List<Vector2> occupiedPositions = new List<Vector2>();
    private GameObject terrainParent;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // Clean up existing terrain
        if (terrainParent != null)
            DestroyImmediate(terrainParent);

        terrainParent = new GameObject("GeneratedTerrain");
        occupiedPositions.Clear();
        Random.InitState(seed);

        PlaceShallows();
        PlaceIslands();
        PlaceRocks();
    }

    public void Clear()
    {
        if (terrainParent != null)
            DestroyImmediate(terrainParent);
        occupiedPositions.Clear();
    }

    // ── SHALLOWS ──────────────────────────────────────────────

    void PlaceShallows()
    {
        for (int i = 0; i < shallowCount; i++)
        {
            Vector2 pos = GetRandomPosition(5f);
            if (pos == Vector2.negativeInfinity) continue;

            float w = Random.Range(shallowMinSize, shallowMaxSize);
            float h = Random.Range(shallowMinSize * 0.6f, shallowMaxSize * 0.6f);
            float rot = Random.Range(0f, 360f);

            GameObject shallow = CreateEllipse("Shallow", pos, new Vector2(w, h), rot,
                                               shallowColor, shallowOrder, true, false);
            shallow.AddComponent<SlowZone>();
        }
    }

    // ── ISLANDS ───────────────────────────────────────────────

    void PlaceIslands()
    {
        int placed = 0;
        int attempts = 0;

        while (placed < islandCount && attempts < 400)
        {
            attempts++;
            Vector2 pos = GetRandomPosition(safeZoneRadius);
            if (pos == Vector2.negativeInfinity) continue;
            if (TooClose(pos, minIslandDistance)) continue;

            float size = Random.Range(islandMinSize, islandMaxSize);
            float w = size;
            float h = size * Random.Range(0.6f, 1f);
            float rot = Random.Range(0f, 360f);

            // Foam ring — slightly larger than island
            CreateEllipse("IslandFoam", pos,
                          new Vector2(w + 0.4f, h + 0.4f), rot,
                          foamColor, foamOrder - 1, false, false);

            // Island body
            GameObject island = CreateEllipse("Island", pos,
                                new Vector2(w, h), rot,
                                islandColor, islandOrder, false, true);

            // Foam particles
            AddFoamParticles(island, Mathf.Max(w, h));

            occupiedPositions.Add(pos);
            placed++;
        }
    }

    // ── ROCKS ─────────────────────────────────────────────────

    void PlaceRocks()
    {
        int placed = 0;
        int attempts = 0;

        while (placed < rockGroupCount && attempts < 400)
        {
            attempts++;
            Vector2 groupPos = GetRandomPosition(safeZoneRadius * 0.8f);
            if (groupPos == Vector2.negativeInfinity) continue;

            // Shallow water base under rocks
            float shallowSize = Random.Range(1.5f, 3f);
            CreateEllipse("RockShallow", groupPos,
                          new Vector2(shallowSize, shallowSize * 0.7f),
                          Random.Range(0f, 360f),
                          shallowColor, shallowOrder, true, false);

            int count = Random.Range(1, rocksPerGroup + 1);
            bool isSingle = count == 1;

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = isSingle ? Vector2.zero :
                    new Vector2(Random.Range(-0.6f, 0.6f),
                                Random.Range(-0.4f, 0.4f));

                Vector2 rockPos = groupPos + offset;

                float w = Random.Range(rockMinSize, rockMaxSize);
                float h = isSingle ?
                    w * Random.Range(0.5f, 1.5f) :
                    w * Random.Range(0.7f, 1.3f);
                float rot = Random.Range(0f, 360f);

                // Foam ring around each rock
                CreateEllipse("RockFoam", rockPos,
                              new Vector2(w + 0.2f, h + 0.2f), rot,
                              foamColor, rockOrder - 1, false, false);

                // Rock body
                GameObject rock = CreateEllipse("Rock", rockPos,
                                  new Vector2(w, h), rot,
                                  rockColor, rockOrder, false, true);
                rock.AddComponent<RockDamage>();
                rock.AddComponent<SlowZone>();

                // Foam particles on larger rocks
                if (Mathf.Max(w, h) > 0.5f)
                    AddFoamParticles(rock, Mathf.Max(w, h) * 0.5f);
            }

            occupiedPositions.Add(groupPos);
            placed++;
        }
    }

    // ── HELPERS ───────────────────────────────────────────────

    GameObject CreateEllipse(string objName, Vector2 pos, Vector2 size,
                             float rotation, Color color, int sortOrder,
                             bool isTrigger, bool solidCollider)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.parent = terrainParent.transform;
        obj.layer = LayerMask.NameToLayer("Terrain");
        obj.transform.position = new Vector3(pos.x, pos.y, 0);
        obj.transform.rotation = Quaternion.Euler(0, 0, rotation);
        obj.transform.localScale = new Vector3(size.x, size.y, 1);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = color;
        sr.sortingOrder = sortOrder;

        if (solidCollider || isTrigger)
        {
            CircleCollider2D col = obj.AddComponent<CircleCollider2D>();
            col.isTrigger = isTrigger;
            col.radius = 0.5f;
        }

        return obj;
    }

    void AddFoamParticles(GameObject parent, float radius)
    {
        GameObject psObj = new GameObject("FoamParticles");
        psObj.transform.parent = parent.transform;
        psObj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = psObj.AddComponent<ParticleSystem>();
        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = rockOrder + 1;
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 1.2f;
        main.startSpeed = 0.1f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(1f, 1f, 1f, 0.7f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = 0f;

        var emission = ps.emission;
        emission.rateOverTime = 8f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;
        shape.radiusThickness = 0.1f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.7f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = grad;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
    }

    Sprite CreateCircleSprite()
    {
        int res = 64;
        Texture2D tex = new Texture2D(res, res);
        Vector2 centre = new Vector2(res / 2f, res / 2f);
        float r = res / 2f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), centre);
                float alpha = Mathf.Clamp01(1f - (dist / r));
                // Soft edge
                alpha = Mathf.SmoothStep(0f, 1f, alpha * 3f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return Sprite.Create(tex,
               new Rect(0, 0, res, res),
               new Vector2(0.5f, 0.5f), res);
    }

    Vector2 GetRandomPosition(float edgePadding)
    {
        for (int i = 0; i < 10; i++)
        {
            float x = Random.Range(-mapWidth / 2 + edgePadding,
                                    mapWidth / 2 - edgePadding);
            float y = Random.Range(-mapHeight / 2 + edgePadding,
                                    mapHeight / 2 - edgePadding);
            Vector2 pos = new Vector2(x, y);

            if (Vector2.Distance(pos, Vector2.zero) >= safeZoneRadius * 0.5f)
                return pos;
        }
        return Vector2.negativeInfinity;
    }

    bool TooClose(Vector2 pos, float minDist)
    {
        foreach (Vector2 existing in occupiedPositions)
        {
            if (Vector2.Distance(pos, existing) < minDist)
                return true;
        }
        return false;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        LevelGenerator gen = (LevelGenerator)target;

        GUILayout.Space(5);
        if (GUILayout.Button("Generate Map"))
        {
            UnityEngine.Random.InitState(gen.seed);
            gen.Generate();
        }
        if (GUILayout.Button("Clear Map"))
        {
            gen.Clear();
        }
    }
}
#endif