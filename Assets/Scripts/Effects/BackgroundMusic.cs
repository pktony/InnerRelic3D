using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void RefreshVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void PlayBGM(MusicClips clip)
    {
        audioSource.clip = SoundManager.Inst.musicClips[(int)clip];
        audioSource.loop = true;
        audioSource.Play();
    }
}
