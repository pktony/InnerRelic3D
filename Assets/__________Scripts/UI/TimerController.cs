using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    Round_UI round_UI;
    RectTransform[] timers;
    int currentRound;

    private readonly Vector2 initialPos = new Vector2(0, -25);
    private readonly Vector2 movePos = new Vector2(-650f, -135f);

    private void Awake()
    {
        timers = new RectTransform[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            timers[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }

        round_UI = GetComponentInParent<Round_UI>();
        round_UI.onVictory += ActivateNextTimer;
    }

    IEnumerator TimerAnimation()
    {
        currentRound = GameManager.Inst.CurrentRound;
        float timer = 0f;
        while(timer < 3.0f)
        {
            timer += Time.deltaTime;
            timers[currentRound - 1].anchoredPosition =
                Vector3.Slerp(timers[currentRound - 1].anchoredPosition,
                movePos,
                Time.deltaTime * 15f);
            yield return null;
        }

        while (GameManager.Inst.roundTimer[currentRound - 1] > 0f)
        {
            timer += Time.deltaTime;
            GameManager.Inst.DecreaseTimer(50f);
            GameManager.Inst.IncreaseTimer(50f);
            timers[currentRound].localScale = Vector2.one *
                (0.5f + 0.5f * Mathf.Abs( Mathf.Sin(timer * 5f)));
            yield return null;
        }

        // Timer Reset
        GameManager.Inst.roundTimer[currentRound - 1] = 0f;
        round_UI.RefreshTimer(currentRound - 1, 0f);
        timers[currentRound].localScale = Vector2.one;
        timers[currentRound - 1].gameObject.SetActive(false);
    }

    private void ActivateNextTimer()
    {
        timers[GameManager.Inst.CurrentRound].gameObject.SetActive(true);
        StartCoroutine(TimerAnimation());
        //timers[GameManager.Inst.CurrentRound - 1].gameObject.SetActive(false);
    }
}
