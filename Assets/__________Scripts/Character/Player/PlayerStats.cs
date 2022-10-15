using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

public class PlayerStats : MonoBehaviour, IHealth, IBattle
{
    Animator anim_Sword;
    Animator anim_Archer;

    CharacterController controller;

    float healthPoint;
    float maxHealthPoint = 100f;
    [SerializeField] float attackPower = 10f;
    float defencePower = 5f;

    private Transform lockonTarget;

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
                Debug.Log("Die");
            }
        }
    }

    public Transform LockonTarget
    {
        get => lockonTarget;
        set
        {
            lockonTarget = value;
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

        // TEST 
        healthPoint = 40f;
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

    public void OnSpcicialAttack()
    {

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


    public void UseInvincibleSkill()
    {
        particles[(int)Skills.Defence].SetActive(true);
        StartCoroutine(MakeInvincible());
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

