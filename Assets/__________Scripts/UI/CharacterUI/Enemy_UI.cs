using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_UI : MonoBehaviour
{
    Enemy enemy;

    // ##### HP #########
    Image hpImg;
    readonly float lerpStoppingPercent = 0.01f;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        enemy.onHealthChange += RefreshHPUI;

        hpImg = GetComponentInChildren<Image>();
    }

    private void Start()
    {
        RefreshHPUI();
    }

    IEnumerator LerpHP()
    {
        while ((hpImg.fillAmount - enemy.HP / enemy.MaxHP) > lerpStoppingPercent)
        {
            hpImg.fillAmount = Mathf.Lerp(hpImg.fillAmount, enemy.HP / enemy.MaxHP, Time.deltaTime * 3.0f);
            yield return null;
        }
    }

    private void RefreshHPUI()
    {
        StartCoroutine(LerpHP());
    }
}
