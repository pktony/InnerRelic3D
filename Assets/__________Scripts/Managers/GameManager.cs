using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    PlayerStats mainPlayer;
    public PlayerStats Player_Stats => mainPlayer;

    PlayerController_Archer archerController;
    public PlayerController_Archer ArcherController => archerController;

    PlayerController_Sword swordController;
    Round_UI roundUI;

    DollyController dollyController;

    private int currentRound = 0;
    private int totalRounds = 3;
    private int enemiesLeft;
    [HideInInspector] public float[] roundTimer;
    [SerializeField] private bool isRoundOver = false;
    [SerializeField] private bool isGameOver = false;

    private int enemyPerRound = 5;
    [SerializeField] private float maxRoundTime = 180f;

    public Action<int, int> onRoundStart;
    public Action startSpawn;
    public Action<int> onEnemyDie;
    public Action<int> onEnemyDieRed;
    public Action<int, float>[] onTimerActivate;    // <currentRound, roundTimer>
    public Action onRoundOver;
    public Action onGameover;

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
                    enemyPerRound = 5;
                    break;
                case 2:
                    enemyPerRound = 10;
                    break;
                case 3:
                    enemyPerRound = 15;
                    break;
            }
        }
    }
    public int TotalRounds => totalRounds;
    public int EnemyPerRound => enemyPerRound;
    public int EnemiesLeft
    {
        get => enemiesLeft;
        set
        {
            if (value < enemiesLeft)
            {
                enemiesLeft = value;
                if (enemiesLeft > 3)
                    onEnemyDie?.Invoke(enemiesLeft);
                else if (EnemiesLeft > 0)
                    onEnemyDieRed?.Invoke(enemiesLeft);
                else if (enemiesLeft == 0)
                {
                    isRoundOver = true;
                    StartCoroutine(SlowMotion());
                    onRoundOver?.Invoke();
                }
            }
            else
            {
                enemiesLeft = value;
                onEnemyDie?.Invoke(enemiesLeft);
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        onTimerActivate = new Action<int, float>[totalRounds];

        mainPlayer = FindObjectOfType<PlayerStats>();
        archerController = FindObjectOfType<PlayerController_Archer>();
        swordController = FindObjectOfType<PlayerController_Sword>();

        roundUI = FindObjectOfType<Round_UI>();

        //dollyController = FindObjectOfType<DollyController>();
        //dollyController.onIntroEnd += RoundStart;

        roundTimer = new float[totalRounds];
        roundTimer[0] = maxRoundTime;
        roundTimer[1] = maxRoundTime;
        roundTimer[2] = maxRoundTime;

        
        //SceneManager.sceneLoaded += Initialize;
    }
    private void Start()
    {
        //dollyController.InitializeIntroUIs();
        currentRound = 0;
        RoundStart();
    }

    private void Initialize(Scene arg0, LoadSceneMode arg1)
    {
        mainPlayer = FindObjectOfType<PlayerStats>();
        archerController = FindObjectOfType<PlayerController_Archer>();
        swordController = FindObjectOfType<PlayerController_Sword>();

        roundUI = FindObjectOfType<Round_UI>();
        roundUI.InitializeRoundUI();

        //dollyController = FindObjectOfType<DollyController>();
        //dollyController.onIntroEnd += RoundStart;

        roundTimer = new float[totalRounds];
        roundTimer[0] = maxRoundTime;
        roundTimer[1] = maxRoundTime;
        roundTimer[2] = maxRoundTime;

        currentRound = 0;
        RoundStart();
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
        onRoundStart.Invoke(currentRound, enemiesLeft);    // 미션 / 라운드 표시

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
            onGameover?.Invoke();
        }
    }

    public void DecreaseTimer(float multiplier = 1f)
    {
        roundTimer[currentRound - 1] -= Time.deltaTime * multiplier;
        onTimerActivate[currentRound - 1]?.Invoke(currentRound - 1, roundTimer[currentRound - 1]);
    }

    public void IncreaseTimer(float multiplier = 1f)
    {
        roundTimer[currentRound] += Time.deltaTime * multiplier;
        onTimerActivate[currentRound]?.Invoke(currentRound, roundTimer[currentRound]);
    }

    IEnumerator SlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 1.0f;
    }
}
