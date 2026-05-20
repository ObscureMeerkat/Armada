using UnityEngine;

public class WakeEffect : MonoBehaviour
{
    private ParticleSystem particles;
    private Rigidbody2D playerRb;

    void Start()
    {
        particles = GetComponent<ParticleSystem>();
        playerRb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        var emission = particles.emission;

        if (playerRb.linearVelocity.magnitude > 0.5f)
        {
            emission.rateOverTime = 20f;
        }
        else
        {
            emission.rateOverTime = 0f;
        }
    }
}