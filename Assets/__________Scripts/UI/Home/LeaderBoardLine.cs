using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderBoardLine : MonoBehaviour
{
    TextMeshProUGUI rankText;

    private void Awake()
    {
        rankText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void SetNewRank(float newScore)
    {
        System.TimeSpan timeSpan = TimeSpan.FromSeconds(newScore);

        rankText.text = string.Format("{0:00} : {1:00} : {2:00}",
            timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds * 0.1f);
    }
}
