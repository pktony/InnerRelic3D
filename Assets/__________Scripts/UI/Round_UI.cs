using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Round_UI : MonoBehaviour
{
    TextMeshProUGUI roundText;
    CanvasGroup victoryButtonGroup;

    private void Awake()
    {
        roundText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        victoryButtonGroup = transform.GetChild(7).GetComponent<CanvasGroup>();
    }

    public void ShowRoundUI()
    {
        victoryButtonGroup.alpha = 1.0f;
        victoryButtonGroup.interactable = true;
        victoryButtonGroup.blocksRaycasts = true;
    }

    public void RefreshRound()
    {
        roundText.text = $"ROUND {GameManager.Inst.CurrentRound}";
    }
}
