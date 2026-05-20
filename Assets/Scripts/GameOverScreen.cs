using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI finalScoreText;
    public GameObject nameInputPanel;
    public TMP_InputField nameInputField;
    public TextMeshProUGUI namePromptText;

    [Header("Audio")]
    public AudioClip gameOverSound;
    private AudioSource audioSource;

    private int finalScore;
    private bool scoreSaved = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && gameOverSound != null)
            audioSource.PlayOneShot(gameOverSound);

        finalScore = ScoreManager.Instance != null ?
                     ScoreManager.Instance.GetScore() : 0;

        if (finalScoreText != null)
            finalScoreText.text = "Score: " + finalScore;

        // Show name input only if high score
        if (nameInputPanel != null)
        {
            bool isHighScore = ScoreManager.Instance != null &&
                               ScoreManager.Instance.IsHighScore(finalScore);
            nameInputPanel.SetActive(isHighScore);
        }
    }

    public void OnSubmitName()
    {
        if (scoreSaved) return;

        string playerName = nameInputField != null &&
                            nameInputField.text.Trim().Length > 0 ?
                            nameInputField.text.Trim() : "Unknown";

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.SaveHighScore(playerName, finalScore);

        scoreSaved = true;

        if (nameInputPanel != null)
            nameInputPanel.SetActive(false);
    }

    public void Restart()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("Armada Game");
    }

    public void MainMenu()
    {
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();
        SceneManager.LoadScene("Title Screen");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options");
    }
}