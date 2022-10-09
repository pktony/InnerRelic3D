using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockonEffect : MonoBehaviour
{
    Transform player;

    private void Start()
    {
        player = GameManager.Inst.Player_Stats.transform;
    }
    private void LateUpdate()
    {
        this.transform.forward = Vector3.ProjectOnPlane(player.position - transform.parent.position, Vector3.up) ;
    }
}
