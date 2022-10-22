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
    SoundManager soundManager;

    public EnemyState status = EnemyState.Idle;
    Animator anim;
    NavMeshAgent agent;
    Rigidbody rigid;
    Transform target = null;
    Material mat;
    AudioSource audioSource;

    [Header("Basic Stats")]
    float healthPoint = 100f;
    float maxHealthPoint = 100f;
    float moveSpeed = 3.0f;

    [Header("AI")]
    [SerializeField] float detectedRange = 5.0f;
    [SerializeField] float attackRange = 1.8f;
    private float defendProbability = 0.05f;
    private bool isDead = false;
    private bool isDefending = false;
    private bool isParry = false;

    private readonly float updateInterval = 0.5f;
    private WaitForSeconds updateSeconds;
    private readonly float blinktime = 0.2f;
    private WaitForSeconds blinkWaitSeconds;

    Collider[] searchColls = new Collider[2]; 

    #region ################# ATTACK
    [Header("Attack")]
    [SerializeField] float attackCoolTime = 5.0f;
    float attackTimer = 0f;
    float attackPower = 10f;
    Quaternion targetAngle = Quaternion.identity;
    #endregion

    #region ################# KnockBack
    private float knockbackForce = 1.0f;
    private float knockbackDuration = 2.0f;
    private WaitForSeconds knockbackWaitSeconds;
    #endregion

    #region IHEALTH
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
    public Action onHealthChange { get; set; }
    #endregion

    #region IBATTLE
    public void Attack(IBattle target)
    {
        if (target != null)
        {
            if (target.IsParry)
            {// 공격한 사람이 넉백 
                ParryAction();
            }
            target.TakeDamage(attackPower);
        }
    }

    public void TakeDamage(float damage)
    {
        if (!isDefending)
        {
            HP -= (damage);
            StartCoroutine(HitBlink());
        }
        else if(isDefending)
        {
            HP += 0f;
        }
        soundManager.PlaySound_Enemy(audioSource, EnemyClip.Hit);
    }

    public bool IsParry { get => isParry;}
    public void ParryAction()
    {
        // 넉백, 잠시 기절
        //Debug.Log($"Enemy Parried");
        ChangeStatus(EnemyState.Knockback);
    }
    
    #endregion

    public float AttackPower => attackPower;
    public Action onDie;

    #region UNITY EVENT 함수 ###################################################
    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        rigid = GetComponent<Rigidbody>();
        rigid.isKinematic = true;
        SkinnedMeshRenderer mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        mat = mesh.material;
        audioSource = GetComponent<AudioSource>();

        updateSeconds = new WaitForSeconds(updateInterval);
        knockbackWaitSeconds = new WaitForSeconds(knockbackDuration);
        blinkWaitSeconds = new WaitForSeconds(blinktime);

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange;
    }

    private void Start()
    {
        soundManager = SoundManager.Inst;
        ChangeStatus(EnemyState.Idle);

        StartCoroutine(StatusCheck());
    }
    #endregion

    #region PRIVATE 함수 ########################################################
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
            float defendRandNum = UnityEngine.Random.value;
            if(defendRandNum < defendProbability)
            {
                anim.SetTrigger("onDefend");
            }
            if (attackTimer > attackCoolTime)
            { // 공격
                int attackNum = UnityEngine.Random.Range(1, 5); // 1 2 3 4
                anim.SetInteger("AttackNum", attackNum);
                anim.SetTrigger("onAttack");
                attackTimer = 0f;
            }
        }
        else
        { // 공격 사거리를 벗어나면 추적 
            ChangeStatus(EnemyState.Track);
        }
    }

    private IEnumerator HitBlink()
    {
        mat.SetColor("_EmissionColor", Color.white);
        yield return blinkWaitSeconds;
        mat.SetColor("_EmissionColor", Color.black);
    }
    void KnockBack()
    {
        anim.SetTrigger("onParried");
        rigid.isKinematic = false;
        Vector3 knockbackDir = transform.position - GameManager.Inst.Player_Stats.transform.position;
        knockbackDir = knockbackDir.normalized;
        rigid.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        StartCoroutine(KnockBackTimer());
    }

    IEnumerator KnockBackTimer()
    {
        yield return knockbackWaitSeconds;
        rigid.isKinematic = true;
        ChangeStatus(EnemyState.Idle);
    }

    /// <summary>
    /// Player 탐지 함수 
    /// </summary>
    /// <returns>True : 범위 내 플레이어 있음  False : 범위 내 플레이어 없음</returns>
    bool SearchPlayer()
    {
        if(Physics.OverlapSphereNonAlloc(
            transform.position, detectedRange, searchColls, LayerMask.GetMask("Player")) > 0)
        {
            target = searchColls[0].transform;
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

    IEnumerator DieProcess()
    {
        isDead = true;
        agent.isStopped = true;
        anim.SetBool("isDead", isDead);
        anim.SetTrigger("onDie");
        onDie?.Invoke();
        gameObject.layer = LayerMask.NameToLayer("Default");    // 타겟 락 방지
        soundManager.PlaySound_Enemy(audioSource, EnemyClip.Die);
        yield return new WaitForSeconds(2.0f);
        Destroy(this.gameObject);
    }

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
                KnockBack();
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
    #endregion

    #region PUBLIC 함수 ################################################
    public void InstantKill()
    {
        ChangeStatus(EnemyState.Die);
    }
    #endregion ########################################################


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