using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_Sword : PlayerController
{
    [SerializeField] private float dizzyTime = 3.0f;

    GameObject invincibleParticles;

    // Special Attack
    private bool isSpinning = false;
    private bool isDizzy = false;
    private float spinTimer = 0f;
    private WaitForSeconds spinWaitSeconds;

    // Invincible Skill
    private float invincibleDuration = 5f;
    private WaitForSeconds invincibleWaitSeconds;


    private TrailRenderer[] swordTrails;
    public Transform swordParent;

    protected override void Awake()
    {
        base.Awake();
        spinWaitSeconds = new WaitForSeconds(1.0f);
        invincibleWaitSeconds = new WaitForSeconds(invincibleDuration);

        swordTrails = swordParent.GetComponentsInChildren<TrailRenderer>();
    }

    private void Start()
    {
        invincibleParticles = Instantiate(GameManager.Inst.Player_Stats.skillDatas[(int)Skills.Defence].skillParticles[0]
            , transform.parent);
        invincibleParticles.SetActive(false);
        ParticleSystem.MainModule mainParticle = invincibleParticles.GetComponent<ParticleSystem>().main;
        mainParticle.duration = invincibleDuration;
    }

    protected override void OnAttack(InputAction.CallbackContext _)
    {
        anim.SetTrigger("onAttack");
        anim.SetFloat("ComboTimer", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
    }

    protected override void OnRightClick(InputAction.CallbackContext obj)
    {// 방어 스킬
        if(GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Defence].IsReadyToUse())
        {
            invincibleParticles.SetActive(true);
            StartCoroutine(MakeInvinible());
        }
    }

    IEnumerator MakeInvinible()
    {
        anim.SetTrigger("onInvincible");
        StartCoroutine(FreezeControl(1.0f));
        controller.detectCollisions = false;
        GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Defence].ResetCoolTime();
        yield return invincibleWaitSeconds;
        controller.detectCollisions = true;
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
