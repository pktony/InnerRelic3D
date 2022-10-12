using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            //Debug.Log(other.name);
            if (other.TryGetComponent<IBattle>(out var target))
            {// Enemy의 Ibattle
                GameManager.Inst.Player_Stats.Attack(target);
            }
        }
    }
}
