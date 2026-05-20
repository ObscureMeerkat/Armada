using UnityEngine;

public class DropEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    public float pulseSpeed = 2f;
    public float pulseMinScale = 1f;
    public float pulseMaxScale = 1.3f;

    [Header("References")]
    public SpriteRenderer glowRenderer;
    public ParticleSystem goldParticles;

    private GameObject glowObject;
    private ParticleSystem ps;

    void Start()
    {
        SetupGlow();
        SetupParticles();
    }

    void SetupGlow()
    {
        glowObject = new GameObject("Glow");
        glowObject.transform.parent = transform;
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localScale = Vector3.one * 1.5f;

        SpriteRenderer sr = glowObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>().sprite;
        sr.color = new Color(1f, 0.85f, 0f, 0.5f);
        sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder - 1;
    }

    void SetupParticles()
    {
        GameObject psObj = new GameObject("GoldParticles");
        psObj.transform.parent = transform;
        psObj.transform.localPosition = Vector3.zero;

        ps = psObj.AddComponent<ParticleSystem>();
        psObj.GetComponent<ParticleSystemRenderer>().material =
            new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        psObj.GetComponent<ParticleSystemRenderer>().sortingOrder = 10;

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 0.8f;
        main.startSpeed = 1f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(1f, 0.85f, 0f, 1f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.3f;

        var emission = ps.emission;
        emission.rateOverTime = 15f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.85f, 0f), 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
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

    void Update()
    {
        if (glowObject == null) return;

        float pulse = Mathf.Lerp(pulseMinScale,
                                  pulseMaxScale,
                                  (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        glowObject.transform.localScale = Vector3.one * pulse * 1.5f;
    }
}