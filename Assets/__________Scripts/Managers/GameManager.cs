using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 게임 로직관련 데이터와 함수가 들어있는 매니저 
/// </summary>
public class GameManager : Singleton<GameManager>
{
    UIManager uiManager;
    SoundManager soundManager;

    private PlayerStats mainPlayer;
    private PlayerController_Archer archerController;
    private PlayerController_Sword swordController;
    
    private DollyController dollyController;
    private CamShaker camShaker;
    private GlobalVolumeController globalVolumeController;

    //  변수 --------------------------------------------------------------------
    private int currentRound = 0;
    private readonly int totalRounds = 3;
    private int enemiesLeft;
    private float[] roundTimer;
    private bool isRoundOver = false;
    private bool isGameOver = false;

    private int enemyPerRound = 5;
    [SerializeField] private float maxRoundTime = 180f;

    // 델리게이트 -----------------------------------------------------------------
    public Action<int, int> onRoundStart;   // <처치해야할 적 숫자, 현재 라운드>
    public Action startSpawn;
    public Action<int> onEnemyDie;          // <남은 적 숫자>
    public Action<int> onEnemyDieRed;       // <남은 적 숫자>
    
    public Action<int> onRoundOver;         // <현재 라운드>
    public Action onGameover;

    // 프로퍼티 ------------------------------------------------------------------
    public PlayerStats Player_Stats => mainPlayer;
    public PlayerController_Archer ArcherController => archerController;
    public CamShaker CamShaker => camShaker;
    public float[] RoundTimer => roundTimer;
    public bool IsRoundOver => isRoundOver;
    public bool IsGameOver
    {   // 게임이 끝났다고 표시할 때 실행될 프로퍼티 
        set
        {
            isGameOver = value;
            onGameover?.Invoke();
            globalVolumeController.ChangeSaturation();
            soundManager.BGMSource.PlayBGM(MusicClips.Gameover);
        }
    }
    public int CurrentRound
    {
        get => currentRound;
        private set
        {
            currentRound = value;
            switch(currentRound)
            {   // 라운드 별 처치해야할 적을 설정 
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
    public int EnemiesLeft
    {
        get => enemiesLeft;
        private set
        {
            if (value < enemiesLeft)
            { // 줄었을 때
                if (!isGameOver)
                {// 게임 오버 됐을 때는 적을 처치해도 줄어들지 않는다 
                    enemiesLeft = value;
                    if (enemiesLeft > 3)
                        onEnemyDie?.Invoke(enemiesLeft);
                    else if (enemiesLeft == 3)
                    {   // 3마리 남았을 때 알림을 표시 
                        uiManager.InfoPanel.ShowPanel("3 Enemies Left. Keep Up");
                        soundManager.PlaySound_UI(UIClips.TimeTicking);
                        onEnemyDieRed?.Invoke(enemiesLeft);
                    }
                    else if (enemiesLeft > 0)
                        onEnemyDieRed?.Invoke(enemiesLeft);
                    else if (enemiesLeft == 0)
                    {   // 모두 처치했을 때 실행할 것들 
                        isRoundOver = true;
                        soundManager.PlaySound_UI(UIClips.Victory);
                        StartCoroutine(SlowMotion());
                        onRoundOver?.Invoke(currentRound);
                        if (currentRound == totalRounds)
                        {
                            SettingManager.Inst.CheckHighScores(roundTimer[totalRounds - 1]);
                            uiManager.LeaderBoard_InGame.RefreshLeaderBoard();
                        }
                    }
                }
            }
            else
            {
                enemiesLeft = value;
                onEnemyDie?.Invoke(enemiesLeft);
            }    
        }
    }

    #region UNITY EVENT 함수 ####################################################
    protected override void Awake()
    {
        base.Awake();

        mainPlayer = FindObjectOfType<PlayerStats>();
        archerController = FindObjectOfType<PlayerController_Archer>();
        swordController = FindObjectOfType<PlayerController_Sword>();
        dollyController = FindObjectOfType<DollyController>();
        dollyController.onIntroEnd += RoundStart;
        camShaker = FindObjectOfType<CamShaker>();
        globalVolumeController = FindObjectOfType<GlobalVolumeController>();
    }

    private void Start()
    {
        uiManager = UIManager.Inst;
        soundManager = SoundManager.Inst;
        dollyController.InitializeIntroUIs();
        mainPlayer.gameObject.SetActive(false);

        // 타이머 초기화 
        roundTimer = new float[totalRounds];
        for(int i = 0; i < totalRounds; i++)
        {
            roundTimer[i] = maxRoundTime;
            uiManager.RefreshTimer(i, roundTimer[i]);
        }
    }
    #endregion

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
        onRoundStart.Invoke(enemyPerRound, enemiesLeft);

        yield return new WaitForSeconds(3.0f);
        startSpawn?.Invoke();   // 몬스터 스폰 시작

        while(!isGameOver && !isRoundOver)
        {
            DecreaseTimer();
            yield return null;
        }
    }

    /// <summary>
    /// 라운드가 끝나고 남은 시간을 다음 타이머에 더해줄 때 실행되는 함수 
    /// </summary>
    /// <param name="multiplier"></param>
    public void DecreaseTimer(float multiplier = 1f)
    {
        if (roundTimer[currentRound - 1] > 0f)
        {
            roundTimer[currentRound - 1] -= Time.deltaTime * multiplier;
            uiManager.RefreshTimer(currentRound - 1, roundTimer[currentRound - 1]);
        }
        else
        {
            IsGameOver = true;
            roundTimer[currentRound - 1] = 0f;
        }
    }

    public void IncreaseTimer(float multiplier = 1f)
    {
        roundTimer[currentRound] += Time.deltaTime * multiplier;
        uiManager.RefreshTimer(currentRound, roundTimer[currentRound]);
    }

    public void ReduceEnemyCount() => EnemiesLeft--;

    /// <summary>
    /// 라운드를 성공했을 때 슬로우 모션 실행 코루틴 
    /// </summary>
    /// <returns></returns>
    private IEnumerator SlowMotion()
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSeconds(1.0f);
        Time.timeScale = 1.0f;
    }
}
