using UnityEngine;

/// <summary>
/// 애니메이션과 소리 타이밍을 맞추기 위해 만든 도우미 클래스 
/// </summary>
public class EnemyAnimatorHelper : MonoBehaviour
{
    SoundManager soundManager;
    AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
    }

    private void Start()
    {
        soundManager = SoundManager.Inst;
    }

    // -------------------------------- Animation 이벤트 함수 
    public void PlayAttackSound1()
    { soundManager.PlaySound_Enemy(audioSource, EnemyClip.Attack1); }

    public void PlayAttackSound2()
    { soundManager.PlaySound_Enemy(audioSource, EnemyClip.Attack2); }

    public void PlayAttackSound3()
    { soundManager.PlaySound_Enemy(audioSource, EnemyClip.Attack3); }
}
