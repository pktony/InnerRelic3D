using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IBattle target = other.GetComponent<IBattle>();
            if (target != null)
            {
                GameManager.Inst.MainPlayer.Attack(target);
            }
        }
    }
}
