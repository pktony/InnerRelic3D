using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_Archer : PlayerController
{
    private LineRenderer lineRend;

    private bool isAiming = false;
    private bool isCharging = false;

    public GameObject arrowPrefab;
    public GameObject arrowSpecial_Prefab;
    public GameObject arrowBezier_Prefab;

    private ShootPositions shootPositions;
    private Transform[] firePosition;
    private int chargeCount = 1;
    private int maxChargeCount = 30;
    private const float MIN_INITIAL_VELOCITY_X = 10f;
    private Vector3 currentVelocity;
    private List<Vector3> trajectoryPoints;

    private WaitForSeconds chargeWaitSeconds;
    private WaitForSeconds bezierWaitSeconds;
    [SerializeField] float bezierInterval = 0.1f;

    public Vector3 CurrentVelocity => currentVelocity;

    #region UNITY EVENT 함수 ###################################################
    protected override void Awake()
    {
        base.Awake();
        trajectoryPoints = new();
        lineRend = transform.GetComponent<LineRenderer>();
        lineRend.useWorldSpace = false;
        lineRend.enabled = false;
        shootPositions = GetComponentInChildren<ShootPositions>();
        shootPositions.InitailizeShootPosistions(maxChargeCount);

        chargeWaitSeconds = new WaitForSeconds(1.0f);
        bezierWaitSeconds = new WaitForSeconds(bezierInterval);
    }

    protected override void Start()
    {
        base.Start();
        firePosition = shootPositions.ActivateShootPositions(1);
    }
    #endregion

    #region PRIVATE 함수 ########################################################
    private IEnumerator CalculateTrajectory()
    {
        // 포물선 방정식에 필요한 최소 변수
        //  - 초기 속도
        //  - 초기 위치
        //  - 시간 또는 거리 
        currentVelocity = Vector3.zero;
        Vector3 currentPosition = firePosition[0].position;
        float elapsedTime = 0f;

        while (isAiming && currentPosition.y > 0f)
        {
            // V = sqrt( V_x^2 + V_y^2) = sqrt(V_0^2 + (gt)^2);
            // vy = gt
            // x = v0t
            // y = 0.5gt^2
            // 거리 = 시간 * 속력

            currentVelocity += MIN_INITIAL_VELOCITY_X * transform.forward + Physics.gravity * elapsedTime;
            currentPosition += currentVelocity * elapsedTime;

            trajectoryPoints.Add(currentPosition);
            lineRend.positionCount = trajectoryPoints.Count;
            lineRend.SetPositions(trajectoryPoints.ToArray());

            elapsedTime += 0.05f;

            yield return new WaitForSeconds(0.05f);
        }
        yield return null;
    }

    /// <summary>
    /// 특수공격 Charging 코루틴 
    /// </summary>
    /// <param name="minimumArrows">최소 발사 화살</param>
    /// <param name="maximumArrows">최대 발사 화살</param>
    /// <param name="perCount">Charging tick 한 번 당 충전될 화살 </param>
    /// <returns></returns>
    private IEnumerator SpecialCharging(int minimumArrows, int maximumArrows, int perCount = 1)
    {
        chargeCount = minimumArrows;
        while (isCharging && chargeCount < maximumArrows)
        {
            chargeCount += perCount;
            //Debug.Log(chargeCount);

            // 파티클
            gameManager.Player_Stats.Particles[2].transform.localPosition = transform.up;
            gameManager.Player_Stats.Particles[2].SetActive(true);
            yield return chargeWaitSeconds;
        }
    }

    private void ShootArrows(int arrowNum, GameObject prefab)
    { // 동시에 여러 발
        for (int i = 0; i < arrowNum; i++)
            Instantiate(prefab, firePosition[i].position, Quaternion.LookRotation(firePosition[i].forward));
        soundManager.PlaySound_Player(audioSource, PlayerClips.NoramlAttack_Archer);
    }

    private IEnumerator ShootBezierArrows(int arrowNum)
    { // 일정 기간동안 여러 발
        int arrowCount = 0;
        while (arrowCount < arrowNum)
        {
            GameObject obj = Instantiate(arrowBezier_Prefab);
            obj.transform.position = firePosition[0].position;
            obj = Instantiate(arrowBezier_Prefab);
            obj.transform.position = firePosition[0].position;
            arrowCount += 5;
            soundManager.PlaySound_Player(audioSource, PlayerClips.SpecialAttack_Bezier);
            yield return bezierWaitSeconds;
        }
    }
    #endregion

    #region PUBLIC 함수 #########################################################
    public void DectectCollision()
    {// 애니메이션 이벤트 함수 
        controller.detectCollisions = true;
    }
    #endregion

    #region PROTECTED 함수 ######################################################
    protected override void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            prevControlMode = gameManager.Player_Stats.ControlMode;
            gameManager.Player_Stats.ControlMode = ControlMode.AimMode;
            isAiming = true;
            anim.SetBool("isAiming", isAiming);
            //lineRend.enabled = true;
            StartCoroutine(CalculateTrajectory());
        }
        else if(context.canceled)
        {
            gameManager.Player_Stats.ControlMode = prevControlMode;
            isAiming = false;
            anim.SetBool("isAiming", isAiming);
            ShootArrows(1, arrowPrefab);
            trajectoryPoints.Clear();
            //lineRend.enabled = false;
        }
    }

    protected override void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (gameManager.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].IsReadyToUse())
        {
            if (context.performed)
            {// Charging 애니메이션까지는 자동으로 넘어간다
                prevControlMode = gameManager.Player_Stats.ControlMode;
                gameManager.Player_Stats.ControlMode = ControlMode.AimMode;
                isCharging = true;
                anim.SetBool("isSpecialAttack", isCharging);
                currentVelocity = MIN_INITIAL_VELOCITY_X * MIN_INITIAL_VELOCITY_X * transform.forward;
                if (gameManager.Player_Stats.LockonTarget == null)
                {// 락온 상태가 아니면 일반 Charging
                    StartCoroutine(SpecialCharging(1, 5));
                }
                else // 베지어 활 Charging
                    StartCoroutine(SpecialCharging(5, maxChargeCount, 10));
            }
            else if (context.canceled)
            {// 특수공격 발사
                gameManager.Player_Stats.ControlMode = prevControlMode;
                isCharging = false;
                anim.SetBool("isSpecialAttack", isCharging);

                if (gameManager.Player_Stats.LockonTarget == null)
                {// 락온 타겟이 없으면 전방에 일정 각도로 퍼지는 공격 
                    shootPositions.ActivateShootPositions(chargeCount);
                    ShootArrows(chargeCount, arrowSpecial_Prefab);
                    shootPositions.ActivateShootPositions(1);  // 원래대로 돌아오기
                }
                else
                {// 락온 타겟이 있으면 베지어 곡선을 따라가는 화살 발사 
                    StartCoroutine(ShootBezierArrows(chargeCount));
                }
                gameManager.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].ResetCoolTime();
            }
        }
    }

    protected override void OnRightClick(InputAction.CallbackContext _)
    {// 회피 기술 사용 
        if(gameManager.Player_Stats.CoolTimes[(int)Skills.Dodge].IsReadyToUse())
        {
            anim.SetTrigger("onDodge");
            gameManager.Player_Stats.CoolTimes[(int)Skills.Dodge].ResetCoolTime();
            controller.detectCollisions = false;    // 사용 시간 동안 무적
        }
    }
    #endregion
}
