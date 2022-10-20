using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController_Sword : PlayerController
{
    [SerializeField] private float dizzyTime = 3.0f;

    // Special Attack
    private bool isSpinning = false;
    private bool isDizzy = false;
    private float spinTimer = 0f;
    private WaitForSeconds spinWaitSeconds;

    private TrailRenderer[] swordTrails;
    public Transform swordParent;

    protected override void Awake()
    {
        base.Awake();

        // Sword 캐릭터 스킬 관련 초기화 
        spinWaitSeconds = new WaitForSeconds(1.0f);
        swordTrails = swordParent.GetComponentsInChildren<TrailRenderer>();
    }

    protected override void OnAttack(InputAction.CallbackContext _)
    {
        base.OnAttack(_);
        if (!gameManager.Player_Stats.IsDead)
        {
            anim.SetTrigger("onAttack");
            anim.SetFloat("ComboTimer", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
        }
    }

    public void PlayRandomAttackSound()
    {// 애니메이션 이벤트 함수 
        int randSound = Random.Range((int)PlayerClips.NormalAttack_Sword1, (int)PlayerClips.NoramlAttack_Sword3 + 1);
        soundManager.PlaySound_Player(audioSource, (PlayerClips)randSound);
    }

    protected override void OnRightClick(InputAction.CallbackContext obj)
    {// 방어 스킬
        gameManager.Player_Stats.UseInvincibleSkill();
    }

    protected override void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (gameManager.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Sword].IsReadyToUse()
            && !gameManager.Player_Stats.IsDead)
        {
            swordTrails[0].enabled = true;
            swordTrails[1].enabled = true;
            if (context.performed)
            {// Charging 애니메이션까지는 자동으로 넘어간다
                isSpinning = true;
                StartCoroutine(SpinTimer());
                soundManager.PlaySound_Player(audioSource, PlayerClips.SpecialAttack_Demacia);
                soundManager.PlaySound_Player(audioSource, PlayerClips.SpecialAttack_Sword, true);
            }
            else if (context.canceled)
            {// 공격 취소
                if (!isDizzy)
                {
                    isSpinning = false;
                    gameManager.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Sword].ResetCoolTime();
                    if (spinTimer > dizzyTime)
                    {
                        isDizzy = true;
                        anim.SetTrigger("onDizzy");
                        StartCoroutine(FreezeControl(2.0f));
                        spinTimer = 0f;
                    }
                }
                swordTrails[0].enabled = false;
                swordTrails[1].enabled = false;
            }
            anim.SetBool("isSpecialAttack", isSpinning);
        }
    }

    private IEnumerator SpinTimer()
    {
        while(isSpinning)
        {
            spinTimer += 1.0f;
            if(spinTimer > dizzyTime )
            {
                gameManager.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Sword].ResetCoolTime();
                isDizzy = true;
                isSpinning = false;
                StartCoroutine(FreezeControl(2.0f));
                spinTimer = 0f;
                anim.SetTrigger("onDizzy");
                anim.SetBool("isSpecialAttack", isSpinning);
                audioSource.loop = false;
            }    
            yield return spinWaitSeconds;
        }
    }

    public IEnumerator FreezeControl(float duration)
    {
        actions.Player.Disable();
        yield return new WaitForSeconds(duration);
        actions.Player.Enable();
        isDizzy = false;
    }
}
