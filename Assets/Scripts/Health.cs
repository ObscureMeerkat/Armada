using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Audio")]
    public AudioClip damageSound;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (damageSound != null && audioSource != null)
            audioSource.PlayOneShot(damageSound);

        if (CompareTag("Enemy"))
        {
            EnemySloop sloop = GetComponent<EnemySloop>();
            if (sloop != null) sloop.AlertEnemy();
        }

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        if (CompareTag("Enemy"))
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(1);

            DropSpawner dropSpawner = FindFirstObjectByType<DropSpawner>();
            if (dropSpawner != null)
                dropSpawner.TrySpawnDrop(transform.position);

            Destroy(gameObject);
        }
        else if (CompareTag("Player"))
        {
            GameOverManager gameOverManager = FindFirstObjectByType<GameOverManager>();
            if (gameOverManager != null)
                gameOverManager.ShowGameOver();
        }
    }
}