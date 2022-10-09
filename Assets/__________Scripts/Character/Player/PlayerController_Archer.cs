using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_Archer : PlayerController
{
    private LineRenderer lineRend;
    private ParticleSystem chargeParticle;

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

    private WaitForSeconds bezierWaitSeconds;
    [SerializeField] float bezierInterval = 0.1f;

    public Vector3 CurrentVelocity => currentVelocity;
    public Transform LockonTarget => lockonTarget;

    protected override void Awake()
    {
        base.Awake();
        trajectoryPoints = new();
        lineRend = transform.GetChild(1).GetComponent<LineRenderer>();
        lineRend.useWorldSpace = false;
        shootPositions = GetComponentInChildren<ShootPositions>();

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
            isAiming = true;
            anim.SetBool("isAiming", isAiming);
            lineRend.enabled = true;
            StartCoroutine(CalculateTrajectory());
        }
        else if(context.canceled)
        {
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

    protected override void SpecialAttack_Performed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {// Charging 애니메이션까지는 자동으로 넘어간다 
            isCharging = true;
            anim.SetBool("isSpecialAttack", isCharging);
            currentVelocity = MIN_INITIAL_VELOCITY_X * MIN_INITIAL_VELOCITY_X * transform.forward;
            if (lockonTarget == null)
                StartCoroutine(SpecialCharging(1, 5));
            else
                StartCoroutine(SpecialCharging(5, 30, 5));
        }
        else if (context.canceled)
        {// 발사
            isCharging = false;
            anim.SetBool("isSpecialAttack", isCharging);

            if (lockonTarget == null)
            {// 락온 타겟이 없으면 전방에 일정 각도로 퍼지는 공격 
                shootPositions.InitializeShootPosition(chargeCount);
                ShootArrows(chargeCount, arrowSpecial_Prefab);
                shootPositions.InitializeShootPosition(1);  // 원래대로 돌아오기
            }
            else
            {// 락온 타겟이 있으면 베지어 곡선을 따라가는 화살 발사 
                StartCoroutine(ShootBezierArrows(chargeCount));
            }
        }
    }

    IEnumerator SpecialCharging(int minimumArrows, int maximumArrows, int perCount = 1)
    {
        chargeCount = minimumArrows;
        while(isCharging && chargeCount < maximumArrows)
        {
            chargeCount++;
            Debug.Log(chargeCount);
            yield return new WaitForSeconds(1.0f);
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
            arrowCount += 2;
            yield return bezierWaitSeconds;
        }
    }

    protected override void OnRightClick(InputAction.CallbackContext obj)
    {//     
        anim.SetTrigger("moveAttack");
        controller.Move(3.0f * transform.forward);
    }

    IEnumerator MoveBackward(float distance)
    {
        while(true)
        {
            controller.Move(-distance * Time.deltaTime * transform.forward);
            yield return null;
        }
    }


#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
#endif
}
