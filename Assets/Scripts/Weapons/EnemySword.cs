using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySword : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            IBattle target = other.GetComponent<IBattle>();
            if (target != null)
            {
                GameManager.Inst.Dog.Attack(target);
            }
        }
    }
}
