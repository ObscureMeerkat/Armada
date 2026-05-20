using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject cannonBallPrefab;
    public Transform firePointLeft;
    public Transform firePointRight;
    public float fireRate = 1f;
    public float spreadAngle = 10f;
    public AudioClip cannonFireSound;

    private float nextFireTime = 0f;
    private PlayerStats playerStats;
    private AudioSource audioSource;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= nextFireTime)
        {
            FireBroadside();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireBroadside()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mousePos.z = 0;

        Vector3 localMousePos = transform.InverseTransformPoint(mousePos);
        Transform firePoint = localMousePos.x >= 0 ? firePointRight : firePointLeft;

        int shotCount = playerStats != null ? playerStats.cannonCount : 1;
        Vector2 baseDirection = (mousePos - firePoint.position).normalized;

        if (shotCount == 1)
        {
            SpawnCannonBall(firePoint.position, baseDirection);
        }
        else
        {
            float totalSpread = spreadAngle * (shotCount - 1);
            float startAngle = -totalSpread / 2f;

            for (int i = 0; i < shotCount; i++)
            {
                float angle = startAngle + (spreadAngle * i);
                Vector2 direction = RotateVector(baseDirection, angle);
                SpawnCannonBall(firePoint.position, direction);
            }
        }

        // Play cannon sound
        if (audioSource != null && cannonFireSound != null)
            audioSource.PlayOneShot(cannonFireSound);
    }

    void SpawnCannonBall(Vector3 spawnPos, Vector2 direction)
    {
        GameObject ball = Instantiate(cannonBallPrefab, spawnPos, Quaternion.identity);
        CannonBall cb = ball.GetComponent<CannonBall>();
        if (cb != null) cb.Launch(direction);
    }

    Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }
}