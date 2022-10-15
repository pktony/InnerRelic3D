using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnerController : MonoBehaviour
{
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
        GameManager.Inst.startSpawn += StartChecking;
        GameManager.Inst.onRoundOver += InstantKillAllEnemies;
    }

    private void Update()
    {// TEST
        if(Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            InstantKillAllEnemies();
        }
    }

    private void StartChecking()
    {
        StartCoroutine(CheckPopulation());
    }

    IEnumerator CheckPopulation()
    {
        while(!GameManager.Inst.IsRoundOver)
        {
            timer += checkInterval;

            if(timer > spawnCoolTime)
            {
                Spawn();
            }

            yield return checkWaitseconds;
        }
    }

    private void InstantKillAllEnemies()
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
            if (!GameManager.Inst.IsRoundOver)
            {
                GameManager.Inst.EnemiesLeft--;
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
