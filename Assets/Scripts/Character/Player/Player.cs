using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Player : MonoBehaviour, IHealth, IBattle
{
    Animator anim;

    float healthPoint = 100f;
    float maxHealthPoint = 100f;
    float attackPower = 10f;
    float defecePower = 5f;

    // Properties #################################
    public float HP
    {
        get => healthPoint;
        set
        {
            healthPoint = Mathf.Clamp(value, 0f, maxHealthPoint);
        }
    }

    public float MaxHP => maxHealthPoint;
    public float AttackPower => attackPower;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
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
}

