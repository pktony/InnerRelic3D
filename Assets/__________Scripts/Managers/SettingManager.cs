using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SettingManager : Singleton<SettingManager>
{
    private float masterVolume = 1.0f;
    public float MasterVol
    {
        get => masterVolume;
        set
        {
            masterVolume = value;
            data.masterVolume = masterVolume;
        }
    }

    private float musicVolume = 1.0f;
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            data.musicVolume = musicVolume;
        }
    }

    private PanelResizer panel;
    public PanelResizer Panel => panel;


    // Save 관련 
    private string saveFileName = "SettingData.json";
    private SettingData data = new();

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += Initialize;
    }

    private void Initialize(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0.buildIndex == 0)
        {
            panel = FindObjectOfType<PanelResizer>();
        }
    }

    public float GetMusicVolume() => masterVolume * musicVolume;

    public void SaveSettingValues()
    {
        // 클래스를 Json 형식으로 전환
        string ToJsonData = JsonUtility.ToJson(data);
        // https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
        string filePath = Application.persistentDataPath + "/" + saveFileName;

        // 파일 생성 
        File.WriteAllText(filePath, ToJsonData);
        print("저장 완료");
    }

    public SettingData LoadSettingValues()
    {
        string filePath = Application.persistentDataPath + "/" + saveFileName;
        float[] loadedValues = new float[2];
        if (File.Exists(filePath))
        {// 이미 있으면
            // 저장된 파일 읽어오고 Json을 클래스 형식으로 전환
            string FromJsonData = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<SettingData>(FromJsonData);
            print("불러오기 완료");
            return data;
        }
        else
        {
            print("불러오기 실패");
            return null;
        }
    }
}
