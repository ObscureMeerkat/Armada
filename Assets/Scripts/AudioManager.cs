using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer audioMixer;
    public AudioSource sfxSource;

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

        LoadSettings();
    }

    public void SetMasterVolume(float sliderValue)
    {
        float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        audioMixer.SetFloat("MasterVolume", db);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        audioMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
    }

    public float GetMasterVolume() => PlayerPrefs.GetFloat("MasterVolume", 1f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 1f);

    void LoadSettings()
    {
        SetMasterVolume(GetMasterVolume());
        SetSFXVolume(GetSFXVolume());
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}