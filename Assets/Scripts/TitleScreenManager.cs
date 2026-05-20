using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("Armada Game");
    }

    public void OnOptionsButtonClicked()
    {
        SceneManager.LoadScene("Options");
    }
}