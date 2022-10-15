using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// 주기적으로 개체수 확인
/// 일정 수를 잡으면 라운드 승리 (게임매니저에 정보 저장 )
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public GameObject Spawn()
    {
        return Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }
}
