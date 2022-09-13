using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Enemy : MonoBehaviour, IHealth, IBattle
{
    //enum AttackState : byte 
    //{
    //    beforeAttack = 0,
    //    duringAttack,
    //    afterAttack
    //}

    //AttackState attackState = AttackState.beforeAttack;
    public EnemyState status = EnemyState.Idle;
    Animator anim;
    NavMeshAgent agent;
    Transform target = null;
    SphereCollider collider;

    [Header("Basic Stats")]
    float healthPoint = 100f;
    float maxHealthPoint = 100f;
    float moveSpeed = 3.0f;

    [Header("AI")]
    [SerializeField] float detectedRange = 5.0f;
    [SerializeField] float attackRange = 2.0f;
    [SerializeField] float timetoPatrol = 1.0f;

    #region PATROL
    Transform[] waypoints;
    #endregion

    #region TRACK
    IEnumerator trackCoroutine;
    WaitForSeconds trackSeconds;
    [SerializeField] float trackCoolTime = 1.0f;
    #endregion

    #region ATTACK
    //IEnumerator attackCoroutine;
    //WaitForSeconds attackCoolTimeSeconds;
    //float attackSeconds;
    float attackTimer = 0f;
    Quaternion targetAngle = Quaternion.identity;
    [Header("Attack")]
    float attackPower = 10f;
    [SerializeField] float attackCoolTime = 5.0f;  // 공격 애니메이션 : 약 1.0f
    #endregion


    public float HP
    {
        get => healthPoint;
        set
        {
            healthPoint = Mathf.Clamp(value, 0, maxHealthPoint);
        }
    }

    public float MaxHP => maxHealthPoint;

    public float AttackPower => attackPower;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        trackCoroutine = UpdateTrackDestination();
        trackSeconds = new WaitForSeconds(trackCoolTime);

        //attackCoroutine = OnAttack();
        //attackCoolTimeSeconds = new WaitForSeconds(attackCoolTime);

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;

        collider = GetComponent<SphereCollider>();
        collider.radius = attackRange;
    }

    private void Start()
    {
        ChangeStatus(EnemyState.Idle);
    }

    private void Update()
    {
        StatusCheck();
    }

    void StatusCheck()
    {
        switch (status)
        {
            case EnemyState.Idle:
                IdleCheck();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Track:
                TrackCheck();
                break;
            case EnemyState.Attack:
                AttackCheck();
                break;
            case EnemyState.Knockback:

                break;
            case EnemyState.Die:
                break;
        }
    }

    void IdleCheck()
    {
        if (SearchPlayer())
            ChangeStatus(EnemyState.Track);
        else
        {
            // 대기 후 Patrol로 이동
            //yield return new WaitForSeconds(timetoPatrol);
            //ChangeStatus(EnemyState.Patrol);
        }
    }

    void Patrol()
    {
        if (SearchPlayer())
            ChangeStatus(EnemyState.Track);
    }

    void TrackCheck()
    {
        if (!SearchPlayer())
        {
            ChangeStatus(EnemyState.Idle);
            return;
        }
        float distanceSqr = (transform.position - target.position).sqrMagnitude;
        if (distanceSqr <= attackRange * attackRange)
        {
            ChangeStatus(EnemyState.Attack);
        }
    }

    void AttackCheck()
    {
        float distanceSqr = (transform.position - target.position).sqrMagnitude;
        if (agent.isStopped && distanceSqr > attackRange * attackRange + 1.0f) // 여유분
        {
            ChangeStatus(EnemyState.Track);
            return;
        }
    }

    //IEnumerator OnAttack()
    //{ //Trigger로 공격 사거리안에 들어가면 시작
    //    while (true)
    //    {
    //        float distanceSqr = (transform.position - target.position).sqrMagnitude;
    //        if (distanceSqr > attackRange * attackRange + 1.0f) // 앞이 플레이어가 아닐때도 구현필요
    //        {// 트리거에 들어가자마자 실행되면서 거리가 더 커지는 경우가 생김
    //            ChangeStatus(EnemyState.Track);
    //            break;
    //        }
    //        anim.SetTrigger("onAttack");
    //        attackState = AttackState.beforeAttack;
    //        attackSeconds = anim.GetCurrentAnimatorClipInfo(0).Length;
    //        yield return new WaitForSeconds(attackSeconds);
    //        attackState = AttackState.afterAttack;
    //        yield return attackCoolTimeSeconds;
    //    }
    //}

    void TrackPlayer()
    {
        if (!agent.pathPending)
        {
            agent.SetDestination(target.position);
        }
    }

    IEnumerator UpdateTrackDestination()
    {
        while (true)
        {
            TrackPlayer();
            yield return trackSeconds;
        }
    }

    bool SearchPlayer()
    {
        Collider[] colls = Physics.OverlapSphere(transform.position, detectedRange, LayerMask.GetMask("Player"));
        if (colls.Length > 0)
        {
            target = colls[0].transform;
            return true;
        }
        target = null;
        return false;
    }

    void LockOn()
    {
        //targetAngle = Quaternion.Euler(target.position - transform.position);
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, targetAngle, Time.deltaTime);
        transform.rotation = Quaternion.LookRotation(target.position - transform.position);
    }

    #region IBATTLE
    public void Attack(IBattle target)
    {
        if (target != null)
        {
            target.TakeDamage(attackPower);
        }
    }

    public void TakeDamage(float damage)
    {
        HP -= damage;
        if (HP > 0f)
        {
            anim.SetTrigger("onHit");
        }
        else
        {
            Debug.Log("Die");
        }
    }
    #endregion

    //private void OnTriggerEnter(Collider other)
    //{// 공격 사거리
    //    if (other.CompareTag("Player"))
    //    {
    //        ChangeStatus(EnemyState.Attack);
    //    }
    //}

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LockOn();
            attackTimer += Time.deltaTime;
            if (attackTimer > attackCoolTime)
            {
                anim.SetTrigger("onAttack");
                attackTimer = 0f;
            }
        }
    }

    // 버그가 많아서 삭제
    //private void OnTriggerExit(Collider other)
    //{// 공격 사거리에서 빠져나가면 Track 상태로 전환  // 버그 심함
    //    if (other.CompareTag("Player"))
    //    {
    //        ChangeStatus(EnemyState.Track);
    //    }
    //}

    #region On Status Entry / Exit
    void ChangeStatus(EnemyState newStatus)
    {
        switch (status)
        {
            case EnemyState.Idle:
                break;
            case EnemyState.Track:
                StopCoroutine(trackCoroutine);
                break;
            case EnemyState.Patrol:
                break;
            case EnemyState.Attack:
                //StopCoroutine(attackCoroutine);
                agent.isStopped = false;
                break;
            case EnemyState.Knockback:
                break;
            case EnemyState.Die:
                break;
        }

        switch (newStatus)
        { // On Status Enter
            case EnemyState.Idle:
                break;
            case EnemyState.Track:
                StartCoroutine(trackCoroutine);
                break;
            case EnemyState.Patrol:
                break;
            case EnemyState.Attack:
                //StartCoroutine(attackCoroutine);
                anim.SetTrigger("onAttack");
                agent.isStopped = true;
                break;
            case EnemyState.Knockback:
                break;
            case EnemyState.Die:
                break;
        }

        anim.SetInteger("CurrentStatus", (int)newStatus);
        status = newStatus;
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.color = Color.white;
        if (status == EnemyState.Track || status == EnemyState.Attack)
        {
            Handles.color = Color.red;
        }
        Handles.DrawWireDisc(transform.position, Vector3.up, detectedRange);

        Handles.color = Color.white;
        Handles.DrawWireDisc(transform.position, Vector3.up, attackRange);
    }
#endif
}