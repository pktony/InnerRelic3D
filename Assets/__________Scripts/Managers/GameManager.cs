using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    UIManager uiManager;

    PlayerStats mainPlayer;
    PlayerController_Archer archerController;
    PlayerController_Sword swordController;
    
    DollyController dollyController;

    //  변수 --------------------------------------------------------------------
    private int currentRound = 0;
    private int totalRounds = 3;
    private int enemiesLeft;
    private float[] roundTimer;
    private bool isRoundOver = false;
    private bool isGameOver = false;

    private int enemyPerRound = 5;
    [SerializeField] private float maxRoundTime = 180f;

    // 델리게이트 -----------------------------------------------------------------
    public Action<int, int> onRoundStart;
    public Action startSpawn;
    public Action<int> onEnemyDie;
    public Action<int> onEnemyDieRed;
    
    public Action<int> onRoundOver;
    public Action onGameover;

    // 프로퍼티 ------------------------------------------------------------------
    public PlayerStats Player_Stats => mainPlayer;
    public PlayerController_Archer ArcherController => archerController;
    public float[] RoundTimer => roundTimer;
    public bool IsRoundOver => isRoundOver;
    public int CurrentRound
    {
        get => currentRound;
        private set
        {
            currentRound = value;
            switch(currentRound)
            {
                case 1:
                    enemyPerRound = 1;
                    break;
                case 2:
                    enemyPerRound = 1;
                    break;
                case 3:
                    enemyPerRound = 1;
                    break;
            }
        }
    }
    public int TotalRounds => totalRounds;
    public int EnemiesLeft
    {
        get => enemiesLeft;
        private set
        {
            if (value < enemiesLeft)
            { // 줄었을 때
                enemiesLeft = value;
                if (enemiesLeft > 3)
                    onEnemyDie?.Invoke(enemiesLeft);
                else if (enemiesLeft == 3)
                {
                    uiManager.InfoPanel.ShowPanel("3 Enemies Left. Keep Up");
                    onEnemyDieRed?.Invoke(enemiesLeft);
                }
                else if (enemiesLeft > 0)
                    onEnemyDieRed?.Invoke(enemiesLeft);
                else if (enemiesLeft == 0)
                {
                    isRoundOver = true;
                    StartCoroutine(SlowMotion());
                    onRoundOver?.Invoke(currentRound);
                }
            }
            else
            { // EnemiesLeft 초기화 시
                enemiesLeft = value;
                onEnemyDie?.Invoke(enemiesLeft);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        mainPlayer = FindObjectOfType<PlayerStats>();
        archerController = FindObjectOfType<PlayerController_Archer>();
        swordController = FindObjectOfType<PlayerController_Sword>();
        dollyController = FindObjectOfType<DollyController>();
        dollyController.onIntroEnd += RoundStart;
    }

    private void Start()
    {
        uiManager = UIManager.Inst;
        dollyController.InitializeIntroUIs();

        // 타이머 초기화 
        roundTimer = new float[totalRounds];
        for(int i = 0; i < totalRounds; i++)
        {
            roundTimer[i] = maxRoundTime;
            uiManager.RefreshTimer(i, roundTimer[i]);
        }
    }

    public void RoundStart()
    {
        CurrentRound++;
        EnemiesLeft = enemyPerRound;
        isRoundOver = false;
        isGameOver = false;
        StartCoroutine(ShowRound());
    }

    private IEnumerator ShowRound()
    {
        onRoundStart.Invoke(enemyPerRound, enemiesLeft);    // 미션 / 라운드 표시

        yield return new WaitForSeconds(3.0f);
        startSpawn?.Invoke();   // 몬스터 스폰 시작

        while(!isGameOver && !isRoundOver)
        {
            StartRoundTimer();
            yield return null;
        }
    }

    private void StartRoundTimer()
    {
        DecreaseTimer();
        if (roundTimer[currentRound - 1] <= 0f)
        {
            isRoundOver = true;
            isGameOver = true;
            roundTimer[currentRound - 1] = 0f;
            onGameover?.Invoke();
        }
    }

    public void DecreaseTimer(float multiplier = 1f)
    {
        roundTimer[currentRound - 1] -= Time.deltaTime * multiplier;
        uiManager.RefreshTimer(currentRound - 1, roundTimer[currentRound - 1]);
    }

    public void IncreaseTimer(float multiplier = 1f)
    {
        roundTimer[currentRound] += Time.deltaTime * multiplier;
        uiManager.RefreshTimer(currentRound, roundTimer[currentRound]);
    }

    public void ReduceEnemyCount() => EnemiesLeft--;

    IEnumerator SlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 1.0f;
    }
}
