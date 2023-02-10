using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Player Controller에서 필요한 함수를 모아놓은 클래스
/// 플레이어 관련 Stat이 저장되어 있다  
/// </summary>
public class PlayerStats : MonoBehaviour, IHealth, IBattle
{
    GameManager gameManager;
    DataManager dataManager;
    SoundManager soundManager;
    ParryingHelper parryingHelper;

    Animator anim_Sword;
    Animator anim_Archer;
    AudioSource audioSource;

    float healthPoint = 5f;
    private Weapons weaponType = Weapons.Sword;

    // ################ Target Lockon
    [SerializeField] float lockOnRadius = 20f;
    private GameObject lockOnEffect;

    // Heal Skill
    private float healAmount;
    private float healTickNum;
    private float healInterval;
    private WaitForSeconds healWaitSeconds;

    // Invincible Skill
    private bool isParry = false;
    WaitForSeconds parrywaitSeconds;
    IEnumerator parryingCoroutine;

    // Particles
    public GameObject[] Particles { get; private set; } 
    #region 프로퍼티 #############################################################
    public float HP
    {
        get => healthPoint;
        set
        {
            if (healthPoint > 0f)
            {
                if (value < healthPoint)
                { // hit
                    anim_Sword.SetTrigger("onHit");
                        anim_Archer.SetTrigger("onHit");
                    PlayRandomHitSound();
                }
                else if(value > healthPoint)
                { // heal
                    Particles[(int)ParticleList.healTick].transform.position = transform.position + Vector3.up;
                    Particles[(int)ParticleList.healTick].SetActive(true);
                }
                else
                { // 방어 중 공격을 받았을 때 
                    anim_Sword.SetTrigger("onHit");
                    PlayRandomHitSound();
                }
                healthPoint = (int)Mathf.Clamp(value, 0f, MaxHP);
                //Debug.Log($"Player HP : {healthPoint}");
                onHealthChange?.Invoke();
            }
            else
            {// 죽을 때 실행
                if (!IsDead)
                {// 연속으로 죽는거 방지 
                    IsDead = true;
                    anim_Sword.SetBool("isDead", IsDead);
                    anim_Archer.SetBool("isDead", IsDead);
                    anim_Archer.SetTrigger("onDie");
                    anim_Sword.SetTrigger("onDie");
                    gameManager.IsGameOver = true;
                }
            }
        }
    }

    public bool IsDead { get; private set; }
    public Vector3 HitPoint { get; set; }
    public Transform lockonFollowTarget { get; private set; }
    public Transform LockonTarget { get; private set; }
    public ControlMode ControlMode { get; set; }

    public float MaxHP { get; private set; }
    public float AttackPower { get; private set; }
    public float DefencePower { get; private set; }
    public float moveSpeed { get; set; }
    public bool IsDefending { get; private set; }
    #endregion

    #region 델리게이트 ###########################################################
    public Action onHealthChange { get; set; }
    #endregion

    #region UNITY EVENT 함수 ###################################################
    private void Awake()
    {
        anim_Sword = transform.GetChild(0).GetComponent<Animator>();
        anim_Archer = transform.GetChild(1).GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // 락온 ----------------------------------------------------------------
        lockonFollowTarget = transform.GetChild(2);
        lockOnEffect = transform.GetChild(4).gameObject;
        lockOnEffect.SetActive(false);
    }
    private void Start()
    {
        gameManager = GameManager.Inst;
        soundManager = SoundManager.Inst;
        dataManager = DataManager.Inst;

        InitializeCharacterStats();
        InitializeCommonSkills();
        //// Particle 초기화 ----------------------------------------------------
        Particles = new GameObject[(int)ParticleList.particleCount];
        for (int i = 0; i < Particles.Length; i++)
        {
            if (i == (int)ParticleList.healTick)
                Particles[i] = InitializeParticles(dataManager.particles[i], false);
            else
                Particles[i] = InitializeParticles(dataManager.particles[i]);
        }

        HP = MaxHP;
    }

    #endregion

    #region IBATTLE
    public bool IsParry => isParry;

    public void Attack(IBattle target)
    {
        if (target != null)
        {
            if(target.IsParry)
            {// 공격한 사람이 넉백 
                ParryAction();
            }
            target.TakeDamage(AttackPower);
        }
    }

    public void TakeDamage(float damage)
    {// 피격 처리 
        if (!IsDefending)
        {// 방어 중이 아닐 때 HP가 깎임 
            HP -= (damage - DefencePower);
        }
        else
        {// 방어 중일때는 피격받지 않고 
            if(weaponType == Weapons.Sword)
            {
                if(isParry)
                {// 패링 성공 시, 상대방에 패링액션을 한다 
                    Particles[(int)ParticleList.specialParrying].transform.position = HitPoint;
                    Particles[(int)ParticleList.specialParrying].SetActive(true);
                    gameManager.CamShaker.ShakeCamera(0.3f, 0.5f);
                    soundManager.PlaySound_Player(audioSource, PlayerClips.Defence);
                    isParry = false;
                }
                HP += 0f;
            }
        }
    }

    public void ParryAction()
    {
        //넉백, 잠시 기절
        // 적은 패링 없음
        Debug.Log("Parrying Activated");
    }
    #endregion

    #region PUBLIC 함수 #########################################################
    public void SetWeapon(Weapons newWeapon) => weaponType = newWeapon;

    public void Heal()
    {
        if (dataManager.coolTimeDatas[(int)Skills.Heal].IsReadyToUse())
        {
            Particles[(int)ParticleList.healUse].SetActive(true);
            dataManager.coolTimeDatas[(int)Skills.Heal].ResetCoolTime();
            StartCoroutine(SlowHeal());
        }
    }

    public Vector3 GetTargetDirection()
    {// 락온 시 플레이어 뒤에 카메라가 오도록 타겟까지 방향을 구하는 함수 
        if (LockonTarget != null)
        {
            Vector3 dir = transform.position - LockonTarget.position;
            dir = dir.normalized;
            return dir;
        }
        return Vector3.zero;
    }

    public void TargetLock()
    {
        Collider[] lockonCollider = Physics.OverlapSphere(transform.position, lockOnRadius, LayerMask.GetMask("Enemy"));
        // Length 로 확인하려면 nonAlloc을 사용할 수 없음 
        if (lockonCollider.Length > 0)
        {// 락온 대상이 있음 
            if (LockonTarget == null)
            {// 현재 락온돼있는 타겟이 없으면 
                float closestDistance = float.MaxValue;
                foreach (Collider coll in lockonCollider)
                {// 가장 가까운거 찾기 
                    if (coll == null)
                        break;
                    float distanceSqr = (coll.transform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistance)
                    {// 거리가 더 짧을 경우 새로운 값 저장 
                        closestDistance = distanceSqr;
                        LockonTarget = coll.transform;
                    }
                }
                if (LockonTarget != null)
                {
                    Transform target = LockonTarget;
                    if (target.TryGetComponent<Enemy>(out Enemy enemy))
                        enemy.onDie += LockOff;
                    transform.rotation = Quaternion.LookRotation(target.position - transform.position);

                    Transform lockOnEffectParent = target.transform.Find("LockOnEffectPosition");
                    lockOnEffect.transform.parent = lockOnEffectParent;
                    lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);
                    lockOnEffect.SetActive(true);

                    lockonFollowTarget.gameObject.SetActive(true);
                    gameManager.camManager.Lockon(LockonTarget);

                    ControlMode = ControlMode.LockedOn;
                    soundManager.PlaySound_Player(audioSource, PlayerClips.Lockon);
                }
            }
            else
            {// 락온 해제 
                LockOff();
                soundManager.PlaySound_Player(audioSource, PlayerClips.Lockoff);
            }
        }
        else
        {// 락온 타겟이 없다 
            soundManager.PlaySound_Player(audioSource, PlayerClips.LockonFailed);
        }
    }

    public void LockOff()
    {
        if (LockonTarget != null)
        {
            LockonTarget = null;
            lockOnEffect.SetActive(false);
            lockOnEffect.transform.parent = null;

            lockonFollowTarget.gameObject.SetActive(false);
            gameManager.camManager.LockOff();

            ControlMode = ControlMode.Normal;
        }
    }

    public void Defend()
    {
        isParry = true;
        parryingHelper.Coll.enabled = true;
        StartCoroutine(parryingCoroutine);
        IsDefending = true;
    }
    public void UnDefend()
    {
        IsDefending = false;
        StopCoroutine(parryingCoroutine);
        parryingHelper.Coll.enabled = false;
        isParry = false;
    }
    #endregion

    #region PRIVATE 함수 #######################################################
    private void InitializeCharacterStats()
    {
        MaxHP = (float)(dataManager.statDictionary[Weapons.Sword][statType.maxHP]);
        AttackPower = (float)dataManager.statDictionary[Weapons.Sword][statType.attackPower];
        DefencePower = (float)dataManager.statDictionary[Weapons.Sword][statType.defencePower];
        moveSpeed = (float)dataManager.statDictionary[Weapons.Sword][statType.moveSpeed];
    }

    private void InitializeCommonSkills()
    {
        // Heal Skll ----------------------------------------------------------
        var healData = dataManager.skillDictionary[Skills.Heal];
        healAmount = (float)healData[SkillStats.amount];
        healTickNum = (float)healData[SkillStats.tickNum];
        healInterval = (float)healData[SkillStats.interval];
        healWaitSeconds = new WaitForSeconds(healInterval);

        // Sword Defend Skill -------------------------------------------------
        float parryDuration = (float)dataManager.skillDictionary[Skills.Defence][SkillStats.amount];
        parrywaitSeconds = new WaitForSeconds(parryDuration);
        parryingCoroutine = ParryingTimer();
        parryingHelper = GetComponentInChildren<ParryingHelper>();
    }

    private IEnumerator SlowHeal()
    {// 일정 시간 동안 조금씩 힐해주는 함수 
        int counter = 0;
        float healPerTick = healAmount / healTickNum;
        while (counter < healTickNum)
        {
            HP += healPerTick;
            counter++;
            yield return healWaitSeconds;
        }
    }

    private IEnumerator ParryingTimer()
    {// 일정 시간 이후 패링을 비활성화 시키는 함수
        yield return parrywaitSeconds;
        parryingHelper.Coll.enabled = false;
        isParry = false;
    }

    private GameObject InitializeParticles(GameObject particle, bool isPlayerParent = true)
    {// 파티클 초기화(생성) 함수 
        GameObject obj = isPlayerParent ?
            Instantiate(particle, transform) : Instantiate(particle);

        ParticleSystem.MainModule temp = obj.GetComponent<ParticleSystem>().main;
        temp.stopAction = ParticleSystemStopAction.Disable;
        obj.SetActive(false);

        return obj;
    }

    private void PlayRandomHitSound()
    {
        int randSound = UnityEngine.Random.Range((int)PlayerClips.Hit1, (int)PlayerClips.Hit4 + 1);
        soundManager.PlaySound_Player(audioSource, (PlayerClips)randSound);
    }
    #endregion
}