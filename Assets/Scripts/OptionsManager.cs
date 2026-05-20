using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsManager : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("High Score Display")]
    public TextMeshProUGUI[] scoreEntries;

    private bool initialising = true;

    void Start()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance != null ?
                AudioManager.Instance.GetMasterVolume() : 1f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance != null ?
                AudioManager.Instance.GetSFXVolume() : 1f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        initialising = false;
        LoadHighScores();
    }

    public void OnMasterVolumeChanged(float value)
    {
        if (initialising) return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (initialising) return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    void LoadHighScores()
    {
        if (ScoreManager.Instance == null || scoreEntries == null) return;

        List<(string name, int score)> scores =
            ScoreManager.Instance.GetHighScores();

        for (int i = 0; i < scoreEntries.Length && i < scores.Count; i++)
        {
            if (scoreEntries[i] != null)
            {
                scoreEntries[i].text = scores[i].score > 0 ?
                    (i + 1) + ". " + scores[i].name + " - " + scores[i].score :
                    (i + 1) + ". ---";
            }
        }
    }

    public void ReturnToTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title Screen");
    }
}