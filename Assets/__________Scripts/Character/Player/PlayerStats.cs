using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IHealth, IBattle
{
    Animator anim_Sword;
    Animator anim_Archer;

    float healthPoint;
    float maxHealthPoint = 100f;
    [SerializeField] float attackPower = 10f;
    float defencePower = 5f;
    WaitForSeconds healWaitSeconds;

    //Skill Data
    SkillData[] skillDatas;

    HealUI heal_UI;
    

    // Properties #################################
    public float HP
    {
        get => healthPoint;
        set
        {
            healthPoint = Mathf.Clamp(value, 0f, maxHealthPoint);
            if (healthPoint > 0f)
            {
                if (value < healthPoint)
                { // hit
                    anim_Sword.SetTrigger("onHit");
                    anim_Archer.SetTrigger("onHit");
                }
                else
                { // heal
                 
                }
                    Debug.Log($"Player HP : {healthPoint}");
                    onHealthChange?.Invoke();
            }
            else
            {
                Debug.Log("Die");
            }
        }
    }

    public float MaxHP => maxHealthPoint;
    public float AttackPower => attackPower;


    // Delegate ####################################
    public Action onHealthChange { get; set; }


    private void Awake()
    {
        anim_Sword = transform.GetChild(0).GetComponent<Animator>();
        anim_Archer = transform.GetChild(1).GetComponent<Animator>();
        heal_UI = FindObjectOfType<HealUI>();

        skillDatas = new SkillData[(int)Skills.SkillCount]; 
        for(int i = 0; i < (int)Skills.SkillCount; i++)
        {
            //skillDatas[i] = new();
        }

        Transform particleParent = transform.GetChild(3);
        //healParticle = particleParent.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        //heartParticle = particleParent.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();

        //healWaitSeconds = new WaitForSeconds(healInterval);

        healthPoint = 40f;
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

    //public void Heal()
    //{
    //    if (!heal_UI.IsHealing)
    //    {
    //        anim_Sword.SetTrigger("onHeal");
    //        anim_Archer.SetTrigger("onHeal");
    //        healParticle.Play();
    //        heal_UI.IsHealing = true;
    //        StartCoroutine(SlowHeal(healAmount));
    //    }
    //}

    //IEnumerator SlowHeal(float healAmount)
    //{
    //    int counter = 0;
    //    healAmount = healAmount / healTickNum;
    //    while (counter < healTickNum)
    //    {
    //        HP += healAmount;
    //        heartParticle.Play();
    //        counter++;
    //        yield return new WaitForSeconds(1.0f);
    //    }
    //}

    public void OnSpcicialAttack()
    {

    }
}

