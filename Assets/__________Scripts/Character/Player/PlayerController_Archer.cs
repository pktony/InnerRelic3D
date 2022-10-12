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
    private const float MIN_INITIAL_VELOCITY_X = 10f;
    private Vector3 currentVelocity;
    private List<Vector3> trajectoryPoints;

    private WaitForSeconds chargeWaitSeconds;
    private WaitForSeconds bezierWaitSeconds;
    [SerializeField] float bezierInterval = 0.1f;

    public Vector3 CurrentVelocity => currentVelocity;

    protected override void Awake()
    {
        base.Awake();
        trajectoryPoints = new();
        lineRend = transform.GetComponent<LineRenderer>();
        lineRend.useWorldSpace = false;
        shootPositions = GetComponentInChildren<ShootPositions>();

        chargeWaitSeconds = new WaitForSeconds(1.0f);
        bezierWaitSeconds = new WaitForSeconds(bezierInterval);
    }

    private void Start()
    {
        firePosition = shootPositions.InitializeShootPosition(1);
    }

    protected override void OnAttack(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            prevControlMode = controlMode;
            controlMode = ControlMode.AimMode;
            isAiming = true;
            anim.SetBool("isAiming", isAiming);
            //lineRend.enabled = true;
            StartCoroutine(CalculateTrajectory());
        }
        else if(context.canceled)
        {
            controlMode = prevControlMode;
            isAiming = false;
            anim.SetBool("isAiming", isAiming);
            ShootArrows(1, arrowPrefab);
            trajectoryPoints.Clear();
            //lineRend.enabled = false;
        }
    }


    IEnumerator CalculateTrajectory()
    {
        // 포물선 방정식에 필요한 최소 변수
        //  - 초기 속도
        //  - 초기 위치
        //  - 시간 또는 거리 
        currentVelocity = Vector3.zero;
        Vector3 currentPosition = firePosition[0].position;
        float elapsedTime = 0f;

        while(isAiming && currentPosition.y > 0f)
        {
            // V = sqrt( V_x^2 + V_y^2) = sqrt(V_0^2 + (gt)^2);
            // vy = gt
            // x = v0t
            // y = 0.5gt^2
            // 거리 = 시간 * 속력

            currentVelocity += MIN_INITIAL_VELOCITY_X * transform.forward +  Physics.gravity * elapsedTime;
            currentPosition += currentVelocity * elapsedTime;
            

            trajectoryPoints.Add(currentPosition);
            lineRend.positionCount = trajectoryPoints.Count;
            lineRend.SetPositions(trajectoryPoints.ToArray());

            elapsedTime += 0.05f;
            
            yield return new WaitForSeconds(0.05f);
        }
        yield return null;
    }

    protected override void OnSpecialAttack(InputAction.CallbackContext context)
    {
        if (GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].IsReadyToUse())
        {
            if (context.performed)
            {// Charging 애니메이션까지는 자동으로 넘어간다
                prevControlMode = controlMode;
                controlMode = ControlMode.AimMode;
                isCharging = true;
                anim.SetBool("isSpecialAttack", isCharging);
                currentVelocity = MIN_INITIAL_VELOCITY_X * MIN_INITIAL_VELOCITY_X * transform.forward;
                if (GameManager.Inst.Player_Stats.LockonTarget == null) // 일반 특수 활 발사 
                    StartCoroutine(SpecialCharging(1, 5));
                else // 베지어 활 발사 
                    StartCoroutine(SpecialCharging(5, 50, 10));
            }
            else if (context.canceled)
            {// 발사
                controlMode = prevControlMode;
                isCharging = false;
                anim.SetBool("isSpecialAttack", isCharging);

                if (GameManager.Inst.Player_Stats.LockonTarget == null)
                {// 락온 타겟이 없으면 전방에 일정 각도로 퍼지는 공격 
                    shootPositions.InitializeShootPosition(chargeCount);
                    ShootArrows(chargeCount, arrowSpecial_Prefab);
                    shootPositions.InitializeShootPosition(1);  // 원래대로 돌아오기
                }
                else
                {// 락온 타겟이 있으면 베지어 곡선을 따라가는 화살 발사 
                    StartCoroutine(ShootBezierArrows(chargeCount));
                }
                GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.SpecialAttack_Archer].ResetCoolTime();
            }
        }
    }

    IEnumerator SpecialCharging(int minimumArrows, int maximumArrows, int perCount = 1)
    {
        chargeCount = minimumArrows;
        while(isCharging && chargeCount < maximumArrows)
        {
            chargeCount += perCount;
            Instantiate(GameManager.Inst.Player_Stats.Particles[2], transform.position + transform.up, Quaternion.identity);
            Debug.Log(chargeCount);
            yield return chargeWaitSeconds;
        }
    }


    private void ShootArrows(int arrowNum, GameObject prefab)
    { // 동시에 여러 발
        for (int i = 0; i < arrowNum; i++)
            Instantiate(prefab, firePosition[i].position, Quaternion.LookRotation(firePosition[i].forward));
    }

    IEnumerator ShootBezierArrows(int arrowNum)
    { // 일정 기간동안 여러 발
        int arrowCount = 0;
        while (arrowCount < arrowNum)
        {
            GameObject obj = Instantiate(arrowBezier_Prefab);
            obj.transform.position = firePosition[0].position;
            obj = Instantiate(arrowBezier_Prefab);
            obj.transform.position = firePosition[0].position;
            arrowCount += 5;
            yield return bezierWaitSeconds;
        }
    }

    protected override void OnRightClick(InputAction.CallbackContext _)
    {//     
        if(GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Dodge].IsReadyToUse())
        {
            anim.SetTrigger("onDodge");
            GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Dodge].ResetCoolTime();
            controller.detectCollisions = false;
        }
    }

    public void DectectCollision()
    {
        controller.detectCollisions = true;
    }


#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
#endif
}
