using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AudioManager : PersistentSingletonMono<AudioManager>
{
    private Bus _masterBus;
    private Bus _musicBus;
    private Bus _SFXBus;
    private Bus _voiceBus;

    protected override void Awake()
    {
        base.Awake();

        _masterBus = RuntimeManager.GetBus("bus:/");
        _musicBus = RuntimeManager.GetBus("bus:/Music");
        _SFXBus = RuntimeManager.GetBus("bus:/SFX");
        _voiceBus = RuntimeManager.GetBus("bus:/Voice");

        LoadVolumes();
    }

    private void LoadVolumes()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float voiceVolume = PlayerPrefs.GetFloat("VoiceVolume", 1f);

        _masterBus.setVolume(masterVolume);
        _musicBus.setVolume(musicVolume);
        _SFXBus.setVolume(SFXVolume);
        _voiceBus.setVolume(voiceVolume);
    }

    public void SetMasterVolume(float value)
    {
        _masterBus.setVolume(SliderToVolume(value));
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        _musicBus.setVolume(SliderToVolume(value));
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        _SFXBus.setVolume(SliderToVolume(value));
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetVoiceVolume(float value)
    {
        _voiceBus.setVolume(SliderToVolume(value));
        PlayerPrefs.SetFloat("VoiceVolume", value);
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public float GetVoiceVolume()
    {
        return PlayerPrefs.GetFloat("VoiceVolume", 1f);
    }

    private float SliderToVolume(float value)
    {
        return Mathf.Pow(value, 1.5f);
    }
}