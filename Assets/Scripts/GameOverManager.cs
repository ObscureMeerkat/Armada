using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public void ShowGameOver()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game Over");
    }
}