using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameoverUI : MonoBehaviour
{
    CanvasGroup gameoverGroup;
    TextMeshProUGUI gameoverText;

    private void Awake()
    {
        gameoverGroup = GetComponent<CanvasGroup>();
        gameoverText = transform.GetChild(4).GetComponent<TextMeshProUGUI>();
    }

    public void ShowGameoverButtons()
    {
        gameoverGroup.alpha = 1.0f;
        gameoverGroup.interactable = true;
        gameoverGroup.blocksRaycasts = true;
    }

    public void HideGameoverButtons()
    {
        gameoverGroup.alpha = 0f;
        gameoverGroup.interactable = false;
        gameoverGroup.blocksRaycasts = false;
    }

    public IEnumerator ShowGameoverText()
    {// 게임 오버 표시
        SoundManager.Inst.BGMSource.PlayBGM(MusicClips.Gameover);
        Color color = gameoverText.color;
        while (color.a <= 1f)
        {
            color.a += Time.deltaTime;
            gameoverText.color = color;
            yield return null;
        }
        ShowGameoverButtons();
    }
}
