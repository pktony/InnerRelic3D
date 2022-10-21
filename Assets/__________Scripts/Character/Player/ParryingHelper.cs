using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 패링 시 공격 받은 지점을 구해주는 클래스
/// 파티클 시스템 위치에 사용됨 
/// </summary>
public class ParryingHelper : MonoBehaviour
{
    Collider coll;

    public Collider Coll => coll;

    private void Awake()
    {
        coll = GetComponent<Collider>();
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
