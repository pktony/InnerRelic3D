using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
