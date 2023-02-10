using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패링 시 공격 받은 지점을 구해주는 클래스
/// 파티클 시스템 위치에 사용됨 
/// </summary>
public class ParryingHelper : MonoBehaviour
{
    public Collider Coll { get; private set; }

    private void Awake()
    {
        Coll = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            GameManager.Inst.Player_Stats.HitPoint =
                other.ClosestPoint(GameManager.Inst.Player_Stats.transform.position);
        }
    }
}
