using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Cinemachine;


/// <summary>
/// Player Controller에서 필요한 함수를 모아놓은 클래스
/// 플레이어 관련 Stat이 저장되어 있다  
/// </summary>
public class PlayerStats : MonoBehaviour, IHealth, IBattle
{
    GameManager gameManager;
    SoundManager soundManager;
    ParryingHelper parryingHelper;

    Animator anim_Sword;
    Animator anim_Archer;
    AudioSource audioSource;

    [SerializeField] float attackPower = 10f;
    readonly float maxHealthPoint = 100f;
    float healthPoint = 5f;
    float defencePower = 5f;
    bool isDead = false;
    Vector3 hitPoint = Vector3.zero;

    private ControlMode controlMode = ControlMode.Normal;
    private Weapons weaponType = Weapons.Sword;

    private GameObject mainCam;

    // ################ Target Lockon
    [SerializeField] float lockOnRadius = 20f;
    private GameObject lockOnEffect;
    private Transform lockonFollowTarget;
    private Transform lockonTarget;
    protected CinemachineVirtualCamera lockonCam;

    //Skill Data
    CoolTimeManager coolTimeUIManager;
    public SkillData[] skillDatas;
    private CoolTimeData[] coolTimeDatas;
    // [0] heal
    // [1] sword
    // [2] archer
    // [3] dodge

    private GameObject[] particles = new GameObject[4];
    // [0] use
    // [1] tick
    // [2] charge
    // [3] Parrying

    // Heal Skill
    private float healAmount;
    private float healTickNum;
    private float healInterval;
    private WaitForSeconds healWaitSeconds;

    // Invincible Skill
    private bool isDefending = false;
    private bool isParry = false;
    private float parryDuration = 0.5f;
    WaitForSeconds parrywaitSeconds;
    IEnumerator parryingCoroutine;

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
                    particles[1].transform.position = transform.position + Vector3.up;
                    particles[1].SetActive(true);
                }
                else
                { // 방어 중 공격을 받았을 때 
                    anim_Sword.SetTrigger("onHit");
                    PlayRandomHitSound();
                }
                healthPoint = (int)Mathf.Clamp(value, 0f, maxHealthPoint);
                //Debug.Log($"Player HP : {healthPoint}");
                onHealthChange?.Invoke();
            }
            else
            {// 죽을 때 실행
                if (!isDead)
                {// 연속으로 죽는거 방지 
                    isDead = true;
                    anim_Sword.SetBool("isDead", isDead);
                    anim_Archer.SetBool("isDead", isDead);
                    anim_Archer.SetTrigger("onDie");
                    anim_Sword.SetTrigger("onDie");
                    gameManager.IsGameOver = true;
                }
            }
        }
    }

    public bool IsDead => isDead;
    public Vector3 HitPoint
    {
        get => hitPoint;
        set { hitPoint = value; }
    }

    public Transform LockonTarget
    {
        get => lockonTarget;
        set
        {
            lockonTarget = value;
        }
    }
    public ControlMode ControlMode
    {
        get => controlMode;
        set
        {
            controlMode = value;
        }
    }

    public float MaxHP => maxHealthPoint;
    public float AttackPower => attackPower;

    public CoolTimeData[] CoolTimes => coolTimeDatas;
    public GameObject[] Particles => particles;

    public bool IsDefending => isDefending;
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

        // 카메라 ---------------------------------------------------------------
        mainCam = FindObjectOfType<MainCam>(true).gameObject;
        CinemachineVirtualCamera cinemachine = mainCam.GetComponent<CinemachineVirtualCamera>();
        cinemachine.Follow = this.transform;
        cinemachine.LookAt = this.transform;
        lockonCam = FindObjectOfType<LockonCam>(true).GetComponent<CinemachineVirtualCamera>();
        lockonCam.Follow = lockonFollowTarget;
        mainCam.SetActive(false);
        lockonCam.gameObject.SetActive(false);

        // Heal Skll ----------------------------------------------------------
        SkillData_Heal heal = skillDatas[0] as SkillData_Heal;
        healAmount = heal.healAmount;
        healTickNum = heal.healTickNum;
        healInterval = heal.healInterval;
        healWaitSeconds = new WaitForSeconds(healInterval);
        
        // Sword Defend Skill -------------------------------------------------
        parrywaitSeconds = new WaitForSeconds(parryDuration);
        parryingCoroutine = ParryingTimer();
        parryingHelper = GetComponentInChildren<ParryingHelper>();

        // Cool Time Data Initialization --------------------------------------
        coolTimeUIManager = FindObjectOfType<CoolTimeManager>();
        coolTimeUIManager.InitializeUIs();
        coolTimeDatas = new CoolTimeData[(int)Skills.SkillCount];
        for (int i = 0; i < (int)Skills.SkillCount; i++)
        {
            coolTimeDatas[i] = new CoolTimeData(skillDatas[i]);
            coolTimeDatas[i].onCoolTimeChange += coolTimeUIManager[i].RefreshUI;
        }

        // Particle 초기화 ------------------------------------------------------
        // --- 힐 사용 
        particles[0] = InitializeParticles(heal.skillParticles[0]);
        // --- 힐 틱 
        particles[1] = InitializeParticles(heal.skillParticles[1], false);
        // --- 차징 
        particles[2] = InitializeParticles(skillDatas[(int)Skills.SpecialAttack_Archer].skillParticles[0]);
        // --- 패링
        particles[3] = InitializeParticles(skillDatas[(int)Skills.Defence].skillParticles[0]);
    }
    
    private void Start()
    {
        gameManager = GameManager.Inst;
        soundManager = SoundManager.Inst;

        HP = maxHealthPoint;
    }

    private void Update()
    {
        foreach(var data in coolTimeDatas)
        {
            data.CurrentCoolTime -= Time.deltaTime;
        }
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
            target.TakeDamage(attackPower);
        }
    }

    public void TakeDamage(float damage)
    {// 피격 처리 
        if (!isDefending)
        {// 방어 중이 아닐 때 HP가 깎임 
            HP -= (damage - defencePower);
        }
        else
        {// 방어 중일때는 피격받지 않고 
            if(weaponType == Weapons.Sword)
            {
                if(isParry)
                {// 패링 성공 시, 상대방에 패링액션을 한다 
                    particles[3].transform.position = hitPoint;
                    particles[3].SetActive(true);
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
        Debug.Log("Parrying Activated");
    }
    #endregion

    #region PUBLIC 함수 #########################################################
    public void SetWeapon(Weapons newWeapon) => weaponType = newWeapon;

    public void Heal()
    {
        if (coolTimeDatas[(int)Skills.Heal].IsReadyToUse())
        {
            particles[0].SetActive(true);
            coolTimeDatas[(int)Skills.Heal].ResetCoolTime();
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
            if (lockonTarget == null)
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
                        lockonTarget = coll.transform;
                    }
                }
                if (lockonTarget != null)
                {
                    Transform target = lockonTarget;
                    if (target.TryGetComponent<Enemy>(out Enemy enemy))
                        enemy.onDie += LockOff;
                    transform.rotation = Quaternion.LookRotation(target.position - transform.position);

                    Transform lockOnEffectParent = target.transform.Find("LockOnEffectPosition");
                    lockOnEffect.transform.parent = lockOnEffectParent;
                    lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);
                    lockOnEffect.SetActive(true);

                    mainCam.SetActive(false);
                    lockonFollowTarget.gameObject.SetActive(true);
                    lockonCam.LookAt = target;
                    lockonCam.gameObject.SetActive(true);

                    controlMode = ControlMode.LockedOn;
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
            lockonTarget = null;
            lockOnEffect.SetActive(false);
            lockOnEffect.transform.parent = null;

            lockonFollowTarget.gameObject.SetActive(false);
            lockonCam.LookAt = null;
            lockonCam.gameObject.SetActive(false);
            mainCam.transform.position = lockonCam.transform.position;
            mainCam.SetActive(true);

            controlMode = ControlMode.Normal;
        }
    }

    public void Defend()
    {
        isParry = true;
        parryingHelper.Coll.enabled = true;
        StartCoroutine(parryingCoroutine);
        isDefending = true;
    }
    public void UnDefend()
    {
        isDefending = false;
        StopCoroutine(parryingCoroutine);
        parryingHelper.Coll.enabled = false;
        isParry = false;
    }
    #endregion

    #region PRIVATE 함수 #######################################################
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

