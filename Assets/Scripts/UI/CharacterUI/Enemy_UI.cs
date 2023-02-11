using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_UI : MonoBehaviour
{
    // ##### HP #########
    Image hpImg;
    readonly float lerpStoppingPercent = 0.01f;

    private void Awake()
    {
        Enemy enemy = GetComponentInParent<Enemy>();
        enemy.onHealthChange += RefreshHPUI;

        hpImg = GetComponentInChildren<Image>();
    }

    IEnumerator LerpHP(float currentHP, float maxHP)
    {
        while ((hpImg.fillAmount - currentHP / maxHP) > lerpStoppingPercent)
        {
            hpImg.fillAmount = Mathf.Lerp(hpImg.fillAmount, currentHP / maxHP, Time.deltaTime * 3.0f);
            yield return null;
        }
    }

    private void RefreshHPUI(float currentHP, float maxHP)
    {
        StartCoroutine(LerpHP(currentHP, maxHP));
    }
}
