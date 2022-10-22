using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 3개의 스폰 지점을 번갈아 가면서 스폰하기 위해
/// 컨트롤 해주는 클래스  
/// </summary>
public class SpawnerController : MonoBehaviour
{
    GameManager gameManager;
    List<GameObject> enemies = new();

    private EnemySpawner[] spawners;

    private int spawnerIndex = 0;
    
    private float timer = 0f;
    [SerializeField] float spawnCoolTime = 3.0f;

    private float checkInterval = 0.5f;
    private WaitForSeconds checkWaitseconds;

    private void Awake()
    {
        spawners = new EnemySpawner[transform.childCount];

        for(int i = 0; i < transform.childCount; i++)
        {
            spawners[i] = transform.GetChild(i).GetComponent<EnemySpawner>();
        }

        checkWaitseconds = new WaitForSeconds(checkInterval);
    }

    private void Start()
    {
        gameManager = GameManager.Inst;
        gameManager.startSpawn += StartChecking;
        gameManager.onRoundOver += InstantKillAllEnemies;
    }

    private void Update()
    {// TEST
        //if(Keyboard.current.digit3Key.wasPressedThisFrame)
        //{
        //    InstantKillAllEnemies(0);
        //}
    }

    private void StartChecking()
    {
        StartCoroutine(CheckPopulation());
    }

    IEnumerator CheckPopulation()
    {
        while(!gameManager.IsRoundOver)
        {
            timer += checkInterval;

            if(timer > spawnCoolTime)
            {
                Spawn();
            }

            yield return checkWaitseconds;
        }
    }

    /// <summary>
    /// 라운드가 끝날 때 모두 처지하는 함수 
    /// </summary>
    /// <param name="_"> 게임매니저 onRoundOver 델리게이트 등록을 위한 미사용 파라미터</param>
    private void InstantKillAllEnemies(int _)
    {
        foreach(var enemy in enemies)
        {
            Enemy _enemy = enemy.GetComponent<Enemy>();
            _enemy.InstantKill();
        }
        enemies.Clear();
    }

    private void Spawn()
    {
        GameObject obj = spawners[spawnerIndex].Spawn();
        enemies.Add(obj);
        Enemy enemy = obj.GetComponent<Enemy>();
        enemy.onDie += () =>
        {
            if (!gameManager.IsRoundOver)
            {
                gameManager.ReduceEnemyCount();
                enemies.Remove(obj);
            }
        };
        ToNextSpawner();
        timer = 0f;
    }

    /// <summary>
    /// .다음 스포너로 넘어가는 함수 
    /// </summary>
    private void ToNextSpawner()
    {
        spawnerIndex = (spawnerIndex + 1) % spawners.Length;
    }
}
