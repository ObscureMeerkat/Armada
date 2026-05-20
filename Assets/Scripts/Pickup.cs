using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { Cannon, Sails, Armour }
    public PickupType pickupType;
    public float value = 10f;
    public float lifetime = 8f;

    [Header("Audio")]
    public AudioClip pickupSound;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats == null) return;

        switch (pickupType)
        {
            case PickupType.Cannon:
                stats.AddCannon();
                break;
            case PickupType.Sails:
                stats.AddSails(value);
                break;
            case PickupType.Armour:
                stats.AddArmour(value);
                break;
        }

        // Play sound at position before destroying. Prefer AudioManager if available.
        if (pickupSound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(pickupSound);
            else
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }
}