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
        anim.SetTrigger("onAttack");
        anim.SetFloat("ComboTimer", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
    }

    protected override void OnRightClick(InputAction.CallbackContext obj)
    {// 방어 스킬
        if (GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Defence].IsReadyToUse())
        {
            StartCoroutine(FreezeControl(1.0f));
            GameManager.Inst.Player_Stats.UseInvincibleSkill();
        }
    }
    

    protected override void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Sword].IsReadyToUse())
        {
            swordTrails[0].enabled = true;
            swordTrails[1].enabled = true;
            if (context.performed)
            {// Charging 애니메이션까지는 자동으로 넘어간다
                isSpinning = true;
                StartCoroutine(SpinTimer());
            }
            else if (context.canceled)
            {// 발사
                if (!isDizzy)
                {
                    isSpinning = false;
                    GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].ResetCoolTime();
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
                GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].ResetCoolTime();
                isDizzy = true;
                isSpinning = false;
                StartCoroutine(FreezeControl(2.0f));
                spinTimer = 0f;
                anim.SetTrigger("onDizzy");
                anim.SetBool("isSpecialAttack", isSpinning);
            }    
            yield return spinWaitSeconds;
        }
    }

    private IEnumerator FreezeControl(float duration)
    {
        actions.Player.Disable();
        yield return new WaitForSeconds(duration);
        actions.Player.Enable();
        isDizzy = false;
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
#endif
}
