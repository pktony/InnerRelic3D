using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AttackTrigger : MonoBehaviour
{
    Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            if (other.TryGetComponent<IBattle>(out IBattle target))
            {// Player의 Ibattle
                enemy.Attack(target);
            }
        }
    }
}
