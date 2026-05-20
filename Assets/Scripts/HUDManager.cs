using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("HUD Elements")]
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI reinforcementText;
    public TextMeshProUGUI scoreText;
    public UnityEngine.UI.Slider healthBar;

    private Health playerHealth;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player")
                       ?.GetComponent<Health>();

        if (reinforcementText != null)
            reinforcementText.gameObject.SetActive(false);

        SetScore(0);
    }

    void Update()
    {
        if (healthBar != null && playerHealth != null)
            healthBar.value = playerHealth.currentHealth;
    }

    public void SetEnemyCount(int count)
    {
        if (enemyCountText != null)
            enemyCountText.text = "Enemies Remaining: " + count;
    }

    public void SetScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    public void SetReinforcementTimer(int seconds)
    {
        if (reinforcementText != null)
        {
            reinforcementText.gameObject.SetActive(true);
            reinforcementText.text = "Reinforcements Arriving In: " + seconds;
        }
    }

    public void HideReinforcementTimer()
    {
        if (reinforcementText != null)
            reinforcementText.gameObject.SetActive(false);
    }
}