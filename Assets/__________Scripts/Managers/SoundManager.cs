using System;
using System.Collections;
using System.Collections.Generic;
using Boxophobic.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager>
{
    AudioSource audioSource;

    private BackgroundMusic bgmSource;
    public BackgroundMusic BGMSource => bgmSource;

    public AudioClip[] musicClips;
    public AudioClip[] uiClips;
    public AudioClip[] playerClips;
    public AudioClip[] enemyClips;

    private float masterVolume = 1.0f;
    public float MasterVol
    {
        get => masterVolume;
        set
        {
            masterVolume = value;
            SettingManager.Inst.SettingData.masterVolume = masterVolume;
            onVolumeChange?.Invoke(GetMusicVolume());
        }
    }

    private float musicVolume = 1.0f;
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            SettingManager.Inst.SettingData.musicVolume = musicVolume;
            onVolumeChange?.Invoke(GetMusicVolume());
        }
    }

    public System.Action<float> onVolumeChange;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += OnSceneLoad;
        audioSource = GetComponent<AudioSource>();
    }

    private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        bgmSource = FindObjectOfType<BackgroundMusic>();
        onVolumeChange = bgmSource.RefreshVolume;
        if(arg0 == SceneManager.GetSceneByName("Home"))
        {
            bgmSource.PlayBGM(MusicClips.Home);
        }
        else if(arg0 == SceneManager.GetSceneByName("Stage"))
        {
            bgmSource.PlayBGM(MusicClips.RoundStart);
        }
    }

    public void PlaySound_UI(UIClips clip)
    {
        audioSource.PlayOneShot(uiClips[(int)clip], MasterVol * 1.0f);
    }

    public void PlayDragSound()
    {
        if(!audioSource.isPlaying)
            audioSource.PlayOneShot(uiClips[(int)UIClips.Drag], MasterVol * 1.0f);
    }

    public void PlaySound_Player(AudioSource _audioSource, PlayerClips clip, bool isLoop = false)
    {
        if(_audioSource == null)
            _audioSource = audioSource;

        if (!isLoop)
            _audioSource.PlayOneShot(playerClips[(int)clip], masterVolume);
        else
        {
            _audioSource.loop = true;
            _audioSource.clip = playerClips[(int)clip];
            _audioSource.volume = masterVolume;
            _audioSource.Play();
        }    
        
    }

    public float GetMusicVolume() => masterVolume * musicVolume;
}
