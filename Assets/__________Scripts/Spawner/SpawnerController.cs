using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnerController : MonoBehaviour
{
    GameManager gameManager;
    List<GameObject> enemies = new();

    private EnemySpawner[] spawners;

    int spawnerIndex = 0;

    float timer = 0f;
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

    private void ToNextSpawner()
    {
        spawnerIndex = (spawnerIndex + 1) % spawners.Length;
    }
}
