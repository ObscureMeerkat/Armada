using UnityEngine;

public class RockDamage : MonoBehaviour
{
    public float damage = 10f;
    public float damageCooldown = 1f;
    private float nextDamageTime = 0f;

    void OnCollisionStay2D(Collision2D other)
    {
        if (Time.time < nextDamageTime) return;

        if (other.gameObject.CompareTag("Player") ||
            other.gameObject.CompareTag("Enemy"))
        {
            Health health = other.gameObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                nextDamageTime = Time.time + damageCooldown;
            }
        }
    }
}