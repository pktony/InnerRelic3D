using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Round_UI : MonoBehaviour
{
    TextMeshProUGUI roundText;
    TextMeshProUGUI populationText;
    TextMeshProUGUI maxEnemyText;
    TextMeshProUGUI[] timerTexts;
    TextMeshProUGUI gameoverText;
    TextMeshProUGUI instructionText;

    Animator roundAnimator;
    Animator populationAnimator;

    CanvasGroup roundButtonGroup;
    CanvasGroup gameOverGroup;

    [SerializeField] private string startInstruction = "KILL ALL ENEMIES IN TIME";
    [SerializeField] private string roundEndInstrction = "VICTORY !";

    public Action onVictory;
    public Action onInitializationEnd;

    private void Awake()
    {
        roundText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        roundAnimator = roundText.GetComponent<Animator>();
        populationText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        populationAnimator = populationText.GetComponent<Animator>();
        maxEnemyText = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
        gameoverText = transform.GetChild(5).GetComponent<TextMeshProUGUI>();
        instructionText = transform.GetChild(6).GetComponent<TextMeshProUGUI>();

        timerTexts = new TextMeshProUGUI[GameManager.Inst.TotalRounds];
        timerTexts = transform.GetChild(4).GetComponentsInChildren<TextMeshProUGUI>(true);
        //timerTexts[0].text = "Test1";
        //timerTexts[1].text = "Test2";
        //timerTexts[2].text = "Test3";

        roundButtonGroup = transform.GetChild(7).GetComponent<CanvasGroup>();
        gameOverGroup = transform.GetChild(9).GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        InitializeRoundUI();
    }

    public void InitializeRoundUI()
    {
        GameManager.Inst.onRoundStart += OnGameStart;
        GameManager.Inst.onRoundOver += RoundOver_UI;
        GameManager.Inst.onEnemyDie += RefreshPopulationText;
        GameManager.Inst.onGameover += OnGameOver;
        GameManager.Inst.onEnemyDieRed += RefreshPopulationText_Red;

        for (int i = 0; i < GameManager.Inst.TotalRounds; i++)
            GameManager.Inst.onTimerActivate[i] += RefreshTimer;
    }

    // ----------------- 게임 시작 
    private void OnGameStart(int currentRound, int enemies)
    {
        StartCoroutine(ShowStartInstruction(currentRound, enemies));
    }

    IEnumerator ShowStartInstruction(int currentRound, int enemies)
    {// 임무 / 라운드 표시
        maxEnemyText.text = "/   " + GameManager.Inst.EnemyPerRound.ToString();
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
        roundText.text = $"ROUND {currentRound}";
        roundAnimator.SetTrigger("onRoundShow");
        populationText.text = enemies.ToString();

        while (color.a > 0f)
        {// Fade out
            timer -= Time.deltaTime;
            color.a = timer;
            instructionText.color = color;
            yield return null;
        }
    }


    // ----------------- 게임 중 
    private void RefreshPopulationText(int enemiesLeft)
    {
        populationText.text = enemiesLeft.ToString();
        populationAnimator.SetTrigger("onDecrease");
    }

    private void RefreshPopulationText_Red(int enemiesLeft)
    {
        populationText.text = enemiesLeft.ToString();
        populationText.fontMaterial.SetColor("_Color", Color.red);
        populationAnimator.SetTrigger("onDecrease_Red");
    }

    public void RefreshTimer(int currentRound, float timer)
    {
        System.TimeSpan timeSpan = TimeSpan.FromSeconds(timer);

        timerTexts[currentRound].text = string.Format("{0:00} : {1:00} : {2:00}",
            timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds * 0.1f);
    }

    // -------------- 라운드 종료 
    private void RoundOver_UI()
    {
        populationText.text = "0";
        populationAnimator.SetTrigger("onZero");
        StartCoroutine(ShowVictory());
    }

    private void OnGameOver()
    {// 게임오버 ui 표시
        StartCoroutine(ShowGameoverUI());
    }

    IEnumerator ShowGameoverUI()
    {
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

    IEnumerator ShowVictory()
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

        if (GameManager.Inst.CurrentRound != 3)
        {// 다음 라운드 준비
            onVictory?.Invoke();
            roundButtonGroup.alpha = 1.0f;
            roundButtonGroup.interactable = true;
            roundButtonGroup.blocksRaycasts = true;
        }
        else
        {// 게임 끝 
            gameOverGroup.alpha = 1.0f;
            gameOverGroup.interactable = true;
            gameOverGroup.blocksRaycasts = true;
        }
    }
}
