using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : Singleton<SettingManager>
{
    private float masterVolume = 1.0f;
    public float MasterVol
    {
        get => masterVolume;
        set
        {
            masterVolume = value;
        }
    }

    private float musicVolume = 1.0f;
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
        }
    }

    public float GetMusicVolume() => masterVolume * musicVolume;
}
