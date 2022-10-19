using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    GameManager gameManager;
    UIManager uiManager;

    RectTransform[] timers;

    private readonly Vector2 movePos = new Vector2(-650f, -135f);

    private void Awake()
    {
        timers = new RectTransform[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            timers[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        gameManager = GameManager.Inst;
        uiManager = UIManager.Inst;
    }

    IEnumerator TimerAnimation(int currentRound)
    {
        float timer = 0f;
        while(timer < 3.0f)
        {// 타이머 옆으로 잠시 치우기 
            timer += Time.deltaTime;
            timers[currentRound - 1].anchoredPosition =
                Vector3.Slerp(timers[currentRound - 1].anchoredPosition,
                movePos,
                Time.deltaTime * 15f);
            yield return null;
        }

        while (gameManager.RoundTimer[currentRound - 1] > 0f)
        {
            timer += Time.deltaTime;
            gameManager.DecreaseTimer(50f);
            gameManager.IncreaseTimer(50f);
            timers[currentRound].localScale = Vector2.one *
                (0.5f + 0.5f * Mathf.Abs( Mathf.Sin(timer * 5f)));
            yield return null;
        }

        // 타이머 강제 리셋 
        gameManager.RoundTimer[currentRound - 1] = 0f;
        uiManager.RefreshTimer(currentRound - 1, 0f);
        timers[currentRound].localScale = Vector2.one;
        timers[currentRound - 1].gameObject.SetActive(false);
        uiManager.RoundUI.ShowVictoryUI();
    }

    public void ActivateNextTimer(int currentRound)
    {
        timers[currentRound].gameObject.SetActive(true);
        StartCoroutine(TimerAnimation(currentRound));
        //timers[GameManager.Inst.CurrentRound - 1].gameObject.SetActive(false);
    }
}
