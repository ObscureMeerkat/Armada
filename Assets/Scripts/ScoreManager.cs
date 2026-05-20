using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    private int currentScore = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount = 1)
    {
        currentScore += amount;
        if (HUDManager.Instance != null)
            HUDManager.Instance.SetScore(currentScore);
    }

    public int GetScore() => currentScore;

    public void ResetScore() => currentScore = 0;

    // High score management
    public List<(string name, int score)> GetHighScores()
    {
        List<(string, int)> scores = new List<(string, int)>();
        for (int i = 0; i < 5; i++)
        {
            string name = PlayerPrefs.GetString("HS_Name_" + i, "---");
            int score = PlayerPrefs.GetInt("HS_Score_" + i, 0);
            scores.Add((name, score));
        }
        return scores;
    }

    public bool IsHighScore(int score)
    {
        for (int i = 0; i < 5; i++)
        {
            int existing = PlayerPrefs.GetInt("HS_Score_" + i, 0);
            if (score > existing) return true;
        }
        return false;
    }

    public void SaveHighScore(string playerName, int score)
    {
        List<(string name, int score)> scores = GetHighScores();
        scores.Add((playerName, score));
        scores.Sort((a, b) => b.score.CompareTo(a.score));
        if (scores.Count > 5) scores.RemoveRange(5, scores.Count - 5);

        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetString("HS_Name_" + i, scores[i].name);
            PlayerPrefs.SetInt("HS_Score_" + i, scores[i].score);
        }
        PlayerPrefs.Save();
    }
}