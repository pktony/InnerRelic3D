using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player_UI : MonoBehaviour
{
    // ##### HP #########
    Image hpImg;
    TextMeshProUGUI hpText;

    private void Awake()
    {
        PlayerStats player = FindObjectOfType<PlayerStats>();
        player.onHealthChange += RefreshHPUI;

        hpImg = transform.GetChild(0).GetComponent<Image>();
        hpText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    private void RefreshHPUI(float currentHP, float maxHP)
    {
        hpImg.fillAmount = currentHP / maxHP;
        hpText.text = currentHP.ToString();
    }
}
