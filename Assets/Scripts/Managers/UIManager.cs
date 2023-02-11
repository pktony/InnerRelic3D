using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UI Manager에서 해야할 것
///  - 타이머 Text Refresh         (CHECK)
///  - 남은 적 수 Refresh           (CHECK)
///  - 지령 Text 표기 (Gameover, Instruction)  (CHECK)
/// </summary>
public class UIManager : Singleton<UIManager>
{
    GameManager gameManager;

    // 인게임 -------------------------------------------------------------------
    int totalRounds;

    Transform safeAreaUI;

    TextMeshProUGUI instructionText;
    TextMeshProUGUI[] timerTexts;
    TextMeshProUGUI maxEnemyText;

    TextMeshProUGUI populationText;
    Animator populationAnimator;

    TimerController timerController;

    [SerializeField] private string startInstruction = "KILL ALL ENEMIES IN TIME";
    [SerializeField] private string roundEndInstruction = "VICTORY !";

    // 프로퍼티 -----------------------------------------------------------------
    // - 홈 화면
    public LeaderBoard_Home LeaderBoard_Home { get; private set; }
    public SettingMain SettingMain { get; private set; }
    public HomeButtons HomeButtons { get; private set; }
    public InfoPanel InfoPanel { get; private set; }
    // - 인 게임
    public Round_UI RoundUI { get; private set; }
    public LeaderBoard_InGame LeaderBoard_InGame { get; private set; }
    public GameoverUI GameoverUI { get; private set; }

    // 델리게이트 ----------------------------------------------------------------
    public Action<int, float>[] onTimerActivate;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += Initialize;
    }

    private void Initialize(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0 == SceneManager.GetSceneByName("Home"))
        {
            LeaderBoard_Home = FindObjectOfType<LeaderBoard_Home>();
            SettingMain = FindObjectOfType<SettingMain>();
            HomeButtons = FindObjectOfType<HomeButtons>();
        }
        else if(arg0 == SceneManager.GetSceneByName("Stage"))
        {
            gameManager = GameManager.Inst;
            totalRounds = gameManager.TotalRounds;

            RoundUI = FindObjectOfType<Round_UI>();
            InfoPanel = FindObjectOfType<InfoPanel>();
            LeaderBoard_InGame = FindObjectOfType<LeaderBoard_InGame>();

            onTimerActivate = new Action<int, float>[totalRounds];
            for (int i = 0; i < totalRounds; i++)
                onTimerActivate[i] += RefreshTimer;

            timerController = FindObjectOfType<TimerController>();
            timerTexts = new TextMeshProUGUI[totalRounds];
            timerTexts = timerController.GetComponentsInChildren<TextMeshProUGUI>(true);
            
            safeAreaUI = RoundUI.transform.GetChild(0);
            instructionText = safeAreaUI.GetChild(5).GetComponent<TextMeshProUGUI>();
            populationText = safeAreaUI.GetChild(2).GetComponent<TextMeshProUGUI>();
            populationAnimator = populationText.GetComponent<Animator>();
            maxEnemyText = safeAreaUI.GetChild(3).GetComponent<TextMeshProUGUI>();

            GameoverUI = FindObjectOfType<GameoverUI>();
            RoundUI.gameObject.SetActive(false);

            gameManager.onRoundStart += OnGameStart;
            gameManager.onEnemyDie += RefreshPopulationText;
            gameManager.onRoundOver += RoundOver_UI;
            gameManager.onGameover += OnGameOver;
        }
    }

    // 게임 시작 -----------------------------------------------------------------
    public void OnGameStart(int enemyPerRound, int enemies)
    {
        StartCoroutine(ShowStartInstruction(enemyPerRound, enemies));
    }

    IEnumerator ShowStartInstruction(int enemyPerRound, int enemies)
    {// 임무 / 라운드 표시
        RoundUI.gameObject.SetActive(true);
        maxEnemyText.text = "/   " + enemyPerRound.ToString();
        Color color = instructionText.color;
        instructionText.text = startInstruction;

        float timer = 0f;
        while (color.a < 1f)
        {// Fade in
            timer += Time.deltaTime * 10f;
            color.a = timer;
            instructionText.color = color;
            yield return null;
        }
        yield return new WaitForSeconds(1.0f);

        //Show Round Number
        RoundUI.RefreshRoundUI();
        RefreshPopulationText(enemies);

        while (color.a > 0f)
        {// Fade out
            timer -= Time.deltaTime;
            color.a = timer;
            instructionText.color = color;
            yield return null;
        }
    }

    public void RefreshTimer(int currentRound, float timeLeft)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(timeLeft);

        timerTexts[currentRound].text = string.Format("{0:00} : {1:00} : {2:00}",
            timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds * 0.1f);
    }

    // 게임 중 ------------------------------------------------------------------
    public void RefreshPopulationText(int enemiesLeft)
    {
        populationText.text = enemiesLeft.ToString();
        populationAnimator.SetTrigger("onDecrease");
        if(enemiesLeft > 3)
        {
            populationText.color = Color.black;
        }
        else if(enemiesLeft == 3)
        {
            populationText.color = Color.red;
        }
    }

    // 라운드 || 게임 종료 --------------------------------------------------------
    private void RoundOver_UI(int currentRound)
    {
        populationText.text = "0";
        populationAnimator.SetTrigger("onZero");
        StartCoroutine(ShowVictory(currentRound));
    }

    private void OnGameOver()
    {// 게임오버 ui 표시
        StartCoroutine(GameoverUI.ShowGameoverText());
    }

    IEnumerator ShowVictory(int currentRound)
    { // 승리 표시 
        Color color = instructionText.color;
        instructionText.text = roundEndInstruction;

        float timer = 0f;
        while (timer < 3.0f)
        {
            timer += Time.deltaTime;
            color.a += Mathf.Cos(timer * 10f);
            instructionText.color = color;
            yield return null;
        }

        if (currentRound != 3)
        {// 다음 라운드 준비
            timerController.ActivateNextTimer(currentRound);
        }
        else
        {// 게임 끝 
            GameoverUI.ShowGameoverButtons();
        }
    }
}
