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
    int totalRounds;

    TextMeshProUGUI instructionText;
    TextMeshProUGUI[] timerTexts;
    TextMeshProUGUI gameoverText;
    TextMeshProUGUI maxEnemyText;

    TextMeshProUGUI populationText;
    Animator populationAnimator;
    CanvasGroup gameOverGroup;

    Round_UI roundUI;
    InfoPanel infoPanel;
    TimerController timerController;

    [SerializeField] private string startInstruction = "KILL ALL ENEMIES IN TIME";
    [SerializeField] private string roundEndInstrction = "VICTORY !";

    // 프로퍼티 -----------------------------------------------------------------
    public InfoPanel InfoPanel => infoPanel;

    // 델리게이트 ----------------------------------------------------------------
    public Action<int, float>[] onTimerActivate;
    public Action onVictory;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += Initialize;
    }

    private void Initialize(Scene arg0, LoadSceneMode arg1)
    {
        if(arg0 == SceneManager.GetSceneByName("Home"))
        {
        }
        else if(arg0 == SceneManager.GetSceneByName("Stage"))
        {
            gameManager = GameManager.Inst;
            totalRounds = gameManager.TotalRounds;

            roundUI = FindObjectOfType<Round_UI>();
            infoPanel = FindObjectOfType<InfoPanel>();
            onTimerActivate = new Action<int, float>[totalRounds];
            for (int i = 0; i < totalRounds; i++)
                onTimerActivate[i] += RefreshTimer;

            timerController = FindObjectOfType<TimerController>();
            timerTexts = new TextMeshProUGUI[totalRounds];
            timerTexts = timerController.GetComponentsInChildren<TextMeshProUGUI>(true);
            //timerTexts[0].text = "Test1";
            //timerTexts[1].text = "Test2";
            //timerTexts[2].text = "Test3";

            instructionText = roundUI.transform.GetChild(6).GetComponent<TextMeshProUGUI>();
            populationText = roundUI.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            populationAnimator = populationText.GetComponent<Animator>();
            maxEnemyText = roundUI.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            gameoverText = roundUI.transform.GetChild(5).GetComponent<TextMeshProUGUI>();
            gameOverGroup = roundUI.transform.GetChild(9).GetComponent<CanvasGroup>();

            gameManager.onRoundStart += OnGameStart;
            gameManager.onEnemyDie += RefreshPopulationText;
            gameManager.onEnemyDieRed += RefreshPopulationText_Red;
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
        roundUI.RefreshRound();
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
    }

    public void RefreshPopulationText_Red(int enemiesLeft)
    {
        populationText.text = enemiesLeft.ToString();
        populationText.fontMaterial.SetColor("_Color", Color.red);
        populationAnimator.SetTrigger("onDecrease_Red");
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
        StartCoroutine(ShowGameoverUI());
    }

    IEnumerator ShowGameoverUI()
    {// 게임 오버 표시 
        Color color = gameoverText.color;
        while (color.a <= 1f)
        {
            color.a += Time.deltaTime;
            gameoverText.color = color;
            yield return null;
        }
        gameOverGroup.alpha = 1.0f;
        gameOverGroup.interactable = true;
        gameOverGroup.blocksRaycasts = true;
    }

    IEnumerator ShowVictory(int currentRound)
    { // 승리 표시 
        Color color = instructionText.color;
        instructionText.text = roundEndInstrction;

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
            roundUI.ShowRoundUI();
        }
        else
        {// 게임 끝 
            gameOverGroup.alpha = 1.0f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;
        }
    }
}
