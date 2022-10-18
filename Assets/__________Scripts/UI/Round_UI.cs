using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Round_UI : MonoBehaviour
{
    TextMeshProUGUI roundText;
    Animator roundAnimator;
    CanvasGroup victoryButtonGroup;

    private void Awake()
    {
        roundText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        roundAnimator = transform.GetChild(1).GetComponent<Animator>();
        victoryButtonGroup = transform.GetChild(6).GetComponent<CanvasGroup>();
    }

    public void ShowVictoryUI()
    {
        victoryButtonGroup.alpha = 1.0f;
        victoryButtonGroup.interactable = true; 
        victoryButtonGroup.blocksRaycasts = true;
    }

    public void RefreshRoundUI()
    {
        roundText.text = $"ROUND {GameManager.Inst.CurrentRound}";
        roundAnimator.SetTrigger("onRoundShow");
    }
}
