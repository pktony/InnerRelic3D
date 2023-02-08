using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 오디오 클립을 관리하고 사운드 재생 함수가 있는 매니저 
/// </summary>
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

    private System.Action<float> onVolumeChange;

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

    public void PlaySound_Enemy(AudioSource _audioSource, EnemyClip clip, bool isLoop = false)
    {
        if (_audioSource == null)
            _audioSource = audioSource;

        if (!isLoop)
            _audioSource.PlayOneShot(enemyClips[(int)clip], masterVolume);
        else
        {
            _audioSource.loop = true;
            _audioSource.clip = enemyClips[(int)clip];
            _audioSource.volume = masterVolume;
            _audioSource.Play();
        }
    }

    public void StopSound(AudioSource audioSource = null)
    {
        if (audioSource == null)
            audioSource = this.audioSource;
        audioSource.Stop();
        audioSource.loop = false;
    }

    public float GetMusicVolume() => masterVolume * musicVolume;
}
