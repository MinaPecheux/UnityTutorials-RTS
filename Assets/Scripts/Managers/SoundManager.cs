using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    public GameSoundParameters soundParameters;

    public AudioMixer masterMixer;

    private void Start()
    {
        masterMixer.SetFloat("musicVol", soundParameters.musicVolume);
        masterMixer.SetFloat("sfxVol", soundParameters.sfxVolume);
    }

    private void OnEnable()
    {
        EventManager.AddListener("PlaySoundByName", _OnPlaySoundByName);
        EventManager.AddListener("PausedGame", _OnPausedGame);
        EventManager.AddListener("ResumedGame", _OnResumedGame);

        EventManager.AddListener("UpdateGameParameter:musicVolume", _OnUpdateMusicVolume);
        EventManager.AddListener("UpdateGameParameter:sfxVolume", _OnUpdateSfxVolume);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaySoundByName", _OnPlaySoundByName);
        EventManager.RemoveListener("PausedGame", _OnPausedGame);
        EventManager.RemoveListener("ResumedGame", _OnResumedGame);

        EventManager.RemoveListener("UpdateGameParameter:musicVolume", _OnUpdateMusicVolume);
        EventManager.RemoveListener("UpdateGameParameter:sfxVolume", _OnUpdateSfxVolume);
    }

    private void _OnPausedGame()
    {
        StartCoroutine(_TransitioningVolume("musicVol", soundParameters.musicVolume, soundParameters.musicVolume - 6, 0.5f));
        StartCoroutine(_TransitioningVolume("sfxVol", soundParameters.sfxVolume, -80, 0.5f));
    }

    private void _OnResumedGame()
    {
        StartCoroutine(_TransitioningVolume("musicVol", soundParameters.musicVolume - 6, soundParameters.musicVolume, 0.5f));
        StartCoroutine(_TransitioningVolume("sfxVol", -80, soundParameters.sfxVolume, 0.5f));
    }

    private IEnumerator _TransitioningVolume(string volumeParameter, float from, float to, float delay)
    {
        float t = 0;
        while (t < delay)
        {
            masterMixer.SetFloat(volumeParameter, Mathf.Lerp(from, to, t / delay));
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        masterMixer.SetFloat(volumeParameter, to);
    }

    private void _OnPlaySoundByName(object data)
    {
        string clipName = (string) data;

        // try to find the clip in the parameters
        FieldInfo[] fields = typeof(GameSoundParameters).GetFields();
        AudioClip clip = null;
        foreach (FieldInfo field in fields)
        {
            if (field.Name == clipName)
            {
                clip = (AudioClip) field.GetValue(soundParameters);
                break;
            }
        }
        if (clip == null)
        {
            Debug.LogWarning($"Unknown clip name: '{clipName}'");
            return;
        }

        // play the clip
        audioSource.PlayOneShot(clip);
    }

    private void _OnUpdateMusicVolume(object data)
    {
        float volume = (float)data;
        masterMixer.SetFloat("musicVol", volume);
    }

    private void _OnUpdateSfxVolume(object data)
    {
        if (GameManager.instance.gameIsPaused) return;
        float volume = (float)data;
        masterMixer.SetFloat("sfxVol", volume);
    }
}
