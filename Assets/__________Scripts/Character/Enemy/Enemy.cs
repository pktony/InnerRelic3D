using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Enemy : MonoBehaviour, IHealth, IBattle
{
    public EnemyState status = EnemyState.Idle;
    Animator anim;
    NavMeshAgent agent;
    Transform target = null;

    [Header("Basic Stats")]
    float healthPoint = 100f;
    float maxHealthPoint = 100f;
    float moveSpeed = 3.0f;

    [Header("AI")]
    [SerializeField] float detectedRange = 5.0f;
    [SerializeField] float attackRange = 2.0f;
    private bool isDead = false;

    private float updateInterval = 0.5f;
    private WaitForSeconds updateSeconds;

    #region ################# PATROL
    Transform[] waypoints;
    #endregion

    #region ################# ATTACK
    [Header("Attack")]
    [SerializeField] float attackCoolTime = 5.0f;
    float attackTimer = 0f;
    float attackPower = 10f;
    Quaternion targetAngle = Quaternion.identity;
    #endregion

    public float HP
    {
        get => healthPoint;
        set
        {
            healthPoint = Mathf.Clamp(value, 0f, maxHealthPoint);
            if (healthPoint > 0f)
            {
                anim.SetTrigger("onHit");
                //Debug.Log($"Enemy HP : {healthPoint}");
                onHealthChange?.Invoke();
            }
            else
            {
                if (!isDead)
                    ChangeStatus(EnemyState.Die);
            }
        }
    }

    public float MaxHP => maxHealthPoint;

    public float AttackPower => attackPower;

    public Action onHealthChange { get; set; }
    public Action onDie;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();

        updateSeconds = new WaitForSeconds(updateInterval);

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
    }

    private void Start()
    {
        ChangeStatus(EnemyState.Idle);

        StartCoroutine(StatusCheck());
    }

    IEnumerator StatusCheck()
    {
        while (!isDead)
        {
            switch (status)
            {
                case EnemyState.Idle:
                    IdleCheck();
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
            yield return updateSeconds;
        }
    }

    void IdleCheck()
    {
        if(SearchPlayer())
        {   // 탐지 범위 내 있으면 추적 
            ChangeStatus(EnemyState.Track);
        }
    }

    void TrackCheck()
    {
        if (!SearchPlayer())
        {
            ChangeStatus(EnemyState.Idle);
            return;
        }

        if (IsInAttackRange())
            ChangeStatus(EnemyState.Attack);
        else
            TrackPlayer();
    }

    void TrackPlayer()
    {
        if (!agent.pathPending)
        {
            agent.SetDestination(target.position);
        }
    }

    void AttackCheck()
    {
        if (IsInAttackRange() && agent.isStopped)
        {   // 공격 사거리 내 있으면 공격 쿨타임 대기 
            LockOn();
            attackTimer += updateInterval;
            if (attackTimer > attackCoolTime)
            { // 공격 
                anim.SetTrigger("onAttack");
                attackTimer = 0f;
            }
        }
        else
        { // 공격 사거리를 벗어나면 추적 
            ChangeStatus(EnemyState.Track);
        }
    }

    /// <summary>
    /// Player 탐지 함수 
    /// </summary>
    /// <returns>True : 범위 내 플레이어 있음  False : 범위 내 플레이어 없음</returns>
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

    /// <summary>
    /// 공격 사거리를 판단하는 경우는 target이 정해져 있는 경우만 있음 (Track 상태일 때만 사용)
    /// </summary>
    /// <returns>True : 공격사거리에 안, False : 공격사거리 밖 </returns>
    private bool IsInAttackRange()
    {
        return (target.transform.position - transform.position).sqrMagnitude
            < attackRange * attackRange;
    }

    void LockOn()
    {
        transform.rotation = Quaternion.LookRotation(target.position - transform.position);
    }

    public void InstantKill()
    {
        ChangeStatus(EnemyState.Die);
    }

    IEnumerator DieProcess()
    {
        isDead = true;
        agent.isStopped = true;
        anim.SetBool("isDead", isDead);
        anim.SetTrigger("onDie");
        onDie?.Invoke();
        yield return new WaitForSeconds(2.0f);
        Destroy(this.gameObject);
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
    }
    #endregion

    #region On Status Entry / Exit
    void ChangeStatus(EnemyState newStatus)
    {
        switch (status)
        {// On Status Exit
            case EnemyState.Idle:
                break;
            case EnemyState.Track:
                anim.SetBool("isMoving", false);
                break;
            case EnemyState.Attack:
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
                anim.SetBool("isMoving", true);
                break;
            case EnemyState.Attack:
                agent.isStopped = true;
                break;
            case EnemyState.Knockback:
                break;
            case EnemyState.Die:
                if(!isDead)
                    StartCoroutine(DieProcess());
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