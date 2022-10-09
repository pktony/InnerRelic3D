using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player_UI : MonoBehaviour
{
    PlayerStats player;

    // ##### HP #########
    Image hpImg;
    TextMeshProUGUI hpText;

    private void Awake()
    {
        player = FindObjectOfType<PlayerStats>();
        player.onHealthChange += RefreshHPUI;

        hpImg = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        hpText = transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        RefreshHPUI();
    }

    private void RefreshHPUI()
    {
        hpImg.fillAmount = player.HP / player.MaxHP;
        hpText.text = player.HP.ToString();
    }
}
