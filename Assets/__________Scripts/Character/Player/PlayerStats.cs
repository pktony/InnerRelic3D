using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using Cinemachine;

public class PlayerStats : MonoBehaviour, IHealth, IBattle
{
    GameManager gameManager;
    SoundManager soundManager;

    Animator anim_Sword;
    Animator anim_Archer;

    CharacterController controller;
    AudioSource audioSource;

    float healthPoint = 5f;
    readonly float maxHealthPoint = 100f;
    [SerializeField] float attackPower = 10f;
    float defencePower = 5f;
    bool isDead = false;

    protected ControlMode controlMode = ControlMode.Normal;

    protected GameObject mainCam;

    // ################ Target Lockon
    [SerializeField] float lockOnRadius = 20f;
    private GameObject lockOnEffect;
    private Transform lockonFollowTarget;
    private Transform lockonTarget;
    protected CinemachineVirtualCamera lockonCam;

    //Skill Data
    public SkillData[] skillDatas;
    private CoolTimeData[] coolTimeDatas;
    // [0] heal
    // [1] sword
    // [2] archer
    // [3] defence
    // [4] dodge
    private CoolTimeUIManager coolTimeManager;

    private GameObject[] particles = new GameObject[4];
    // [0] use
    // [1] tick
    // [2] charge
    // [3] invincible

    // Heal Skill
    private float healAmount;
    private float healTickNum;
    private float healInterval;
    private WaitForSeconds healWaitSeconds;

    // Invincible Skill
    private float invincibleDuration = 5f;
    private WaitForSeconds invincibleWaitSeconds;

    // Properties #################################
    public float HP
    {
        get => healthPoint;
        set
        {
            if (healthPoint > 0f)
            {
                if (value < healthPoint)
                { // hit
                    //Debug.Log("hit");
                    anim_Sword.SetTrigger("onHit");
                    anim_Archer.SetTrigger("onHit");

                    int randSound = UnityEngine.Random.Range((int)PlayerClips.Hit1, (int)PlayerClips.Hit4 + 1);
                    soundManager.PlaySound_Player(audioSource, (PlayerClips)randSound);
                }
                else
                { // heal
                    Instantiate(particles[1], transform.position + Vector3.up, Quaternion.identity);
                }
                healthPoint = (int)Mathf.Clamp(value, 0f, maxHealthPoint);
                //Debug.Log($"Player HP : {healthPoint}");
                onHealthChange?.Invoke();
            }
            else
            {
                if (!isDead)
                {
                    isDead = true;
                    anim_Sword.SetBool("isDead", isDead);
                    anim_Archer.SetBool("isDead", isDead);
                    anim_Archer.SetTrigger("onDie");
                    anim_Sword.SetTrigger("onDie");
                    GameManager.Inst.IsGameOver = true;
                    Debug.Log("Die");
                }
            }
        }
    }

    public bool IsDead => isDead;

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


    // Delegate ####################################
    public Action onHealthChange { get; set; }


    private void Awake()
    {
        anim_Sword = transform.GetChild(0).GetComponent<Animator>();
        anim_Archer = transform.GetChild(1).GetComponent<Animator>();

        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        // 락온 ----------------------------------------------------
        lockonFollowTarget = transform.GetChild(2);
        lockOnEffect = transform.GetChild(4).gameObject;
        lockOnEffect.SetActive(false);

        // 카메라 --------------------------
        mainCam = FindObjectOfType<MainCam>(true).gameObject;
        CinemachineVirtualCamera cinemachine = mainCam.GetComponent<CinemachineVirtualCamera>();
        cinemachine.Follow = this.transform;
        cinemachine.LookAt = this.transform;
        lockonCam = FindObjectOfType<LockonCam>(true).GetComponent<CinemachineVirtualCamera>();
        lockonCam.Follow = lockonFollowTarget;
        mainCam.SetActive(false);
        lockonCam.gameObject.SetActive(false);

        // Heal Skll --------------------------------------------------
        SkillData_Heal heal = skillDatas[0] as SkillData_Heal;
        healAmount = heal.healAmount;
        healTickNum = heal.healTickNum;
        healInterval = heal.healInterval;
        healWaitSeconds = new WaitForSeconds(healInterval);
        particles[0] = heal.skillParticles[0];
        particles[1] = heal.skillParticles[1];

        // Sword Invincible Skill -------------------------------------
        particles[2] = skillDatas[2].skillParticles[0];
        particles[3] = skillDatas[(int)Skills.Defence].skillParticles[0];
        GameObject obj = Instantiate(particles[3], transform);
        obj.SetActive(false);
        ParticleSystem.MainModule mainParticle = obj.GetComponent<ParticleSystem>().main;
        mainParticle.duration = invincibleDuration;
        invincibleWaitSeconds = new WaitForSeconds(invincibleDuration);

        // Cool Time Data Initialization -----------------------------
        coolTimeManager = FindObjectOfType<CoolTimeUIManager>();
        coolTimeManager.InitializeUIs();
        coolTimeDatas = new CoolTimeData[(int)Skills.SkillCount];
        for (int i = 0; i < (int)Skills.SkillCount; i++)
        {
            coolTimeDatas[i] = new CoolTimeData(skillDatas[i]);
            coolTimeDatas[i].onCoolTimeChange += coolTimeManager[i].RefreshUI;
        }
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
        HP -= (damage - defencePower);
    }
    #endregion

    public void Heal()
    {
        if (coolTimeDatas[(int)Skills.Heal].IsReadyToUse())
        {
            Instantiate(particles[0], transform);
            coolTimeDatas[(int)Skills.Heal].ResetCoolTime();
            StartCoroutine(SlowHeal());
        }
    }

    IEnumerator SlowHeal()
    {
        int counter = 0;
        float healPerTick = healAmount / healTickNum;
        while (counter < healTickNum)
        {
            HP += healPerTick;
            counter++;
            yield return healWaitSeconds;
        }
    }

    public Vector3 GetTargetDirection()
    {
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
            {
                float closestDistance = float.MaxValue;
                foreach (Collider coll in lockonCollider)
                {
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
                    target.GetComponent<Enemy>().onDie += LockOff;
                    transform.rotation = Quaternion.LookRotation(target.position - transform.position);

                    Transform lockOnEffectParent = target.transform.Find("LockOnEffectPosition");
                    lockOnEffect.transform.parent = lockOnEffectParent;
                    lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);

                    lockOnEffect.SetActive(true);

                    mainCam.SetActive(false);   //
                    lockonFollowTarget.gameObject.SetActive(true);  //
                    lockonCam.LookAt = target;  //
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
        gameManager.Player_Stats.LockonTarget = null;
        //lockOnEffect.transform.parent = this.transform;

        lockOnEffect.SetActive(false);

        lockonFollowTarget.gameObject.SetActive(false);
        lockonCam.LookAt = null;
        lockonCam.gameObject.SetActive(false);
        mainCam.transform.position = lockonCam.transform.position;
        mainCam.SetActive(true);

        controlMode = ControlMode.Normal;
    }


    public void UseInvincibleSkill()
    {
        if (coolTimeDatas[(int)Skills.Defence].IsReadyToUse()
            && !isDead)
        {
            particles[(int)Skills.Defence].SetActive(true);
            StartCoroutine(MakeInvincible());
            StartCoroutine(gameManager.SwordController.FreezeControl(1.0f));
        }
    }

    IEnumerator MakeInvincible()
    {
        anim_Sword.SetTrigger("onInvincible");
        controller.detectCollisions = false;
        GameManager.Inst.Player_Stats.CoolTimes[(int)Skills.Defence].ResetCoolTime();
        yield return invincibleWaitSeconds;
        controller.detectCollisions = true;
    }
}

