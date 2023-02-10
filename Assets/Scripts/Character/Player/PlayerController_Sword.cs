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

    #region 애니메이션 string 변수 캐싱 
    private readonly int OnDizzy = Animator.StringToHash("onDizzy");
    private readonly int IsSpecialAttack = Animator.StringToHash("isSpecialAttack");
    private readonly int Attack = Animator.StringToHash("onAttack");
    private readonly int ComboTimer = Animator.StringToHash("ComboTimer");
    private readonly int IsDefending = Animator.StringToHash("isDefending");
    #endregion

    #region UNITY EVENT 함수 ####################################################
    protected override void Awake()
    {
        base.Awake();
        // Sword 캐릭터 스킬 관련 초기화 
        spinWaitSeconds = new WaitForSeconds(1.0f);
        swordTrails = swordParent.GetComponentsInChildren<TrailRenderer>();
    }
    #endregion

    #region PUBLIC 함수 #########################################################
    public void PlayRandomAttackSound()
    {// 애니메이션 이벤트 함수
        int randSound = Random.Range((int)PlayerClips.NormalAttack_Sword1, (int)PlayerClips.NoramlAttack_Sword3 + 1);
        soundManager.PlaySound_Player(audioSource, (PlayerClips)randSound);
    }
    #endregion

    #region PRIVATE 함수 ########################################################
    private IEnumerator SpinTimer()
    {
        while (isSpinning)
        {
            spinTimer += 1.0f;
            if (spinTimer > dizzyTime)
            {
                DataManager.Inst.coolTimeDatas[(int)Skills.SpecialAttack_Sword].ResetCoolTime();
                isDizzy = true;
                isSpinning = false;
                StartCoroutine(FreezeControl(2.0f));
                spinTimer = 0f;
                anim.SetTrigger(OnDizzy);
                anim.SetBool(IsSpecialAttack, isSpinning);
                audioSource.loop = false;
                audioSource.Stop();
                swordTrails[0].enabled = false;
                swordTrails[1].enabled = false;
                break;
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
    #endregion

    #region PROTECTED 함수 ######################################################
    protected override void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !playerStats.IsDead)
        {
            prevControlMode = playerStats.ControlMode;
            playerStats.ControlMode = ControlMode.AimMode;
            anim.SetTrigger(Attack);
            anim.SetFloat(ComboTimer, Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
            GameManager.Inst.CamShaker.ShakeCamera(1.0f, 0.3f, CinemachineImpulseDefinition.ImpulseShapes.Bump);
        }
        else if (context.canceled)
        {
            playerStats.ControlMode = prevControlMode;
        }
    }

    protected override void OnRightClick(InputAction.CallbackContext context)
    {// 방어 스킬
        if (context.performed)
        {
            prevControlMode = playerStats.ControlMode;
            playerStats.ControlMode = ControlMode.AimMode;
            playerStats.Defend();
            anim.SetBool(IsDefending, true);
        }
        else if (context.canceled)
        {
            playerStats.ControlMode = prevControlMode;
            playerStats.UnDefend();
            anim.SetBool(IsDefending, false);
        }
    }

    protected override void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (DataManager.Inst.coolTimeDatas[(int)Skills.SpecialAttack_Sword].IsReadyToUse()
            && !playerStats.IsDead)
        {
            if (context.performed)
            {
                isSpinning = true;
                swordTrails[0].enabled = true;
                swordTrails[1].enabled = true;
                StartCoroutine(SpinTimer());
                soundManager.PlaySound_Player(audioSource, PlayerClips.SpecialAttack_Demacia);
                soundManager.PlaySound_Player(audioSource, PlayerClips.SpecialAttack_Sword, true);
            }
            else if (context.canceled)
            {// 공격 취소
                if (!isDizzy)
                {
                    isSpinning = false;
                }
                DataManager.Inst.coolTimeDatas[(int)Skills.SpecialAttack_Sword].ResetCoolTime();
                swordTrails[0].enabled = false;
                swordTrails[1].enabled = false;
                audioSource.Stop();
                spinTimer = 0f;
            }
            anim.SetBool(IsSpecialAttack, isSpinning);
        }
    }
    #endregion
}
