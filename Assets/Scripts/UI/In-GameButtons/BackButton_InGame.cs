using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton_InGame : BackButton_Base
{
    protected override void HideContent()
    {
        uiManager.LeaderBoard_InGame.HideLeaderBoard();
        uiManager.GameoverUI.ShowGameoverButtons();
    }
}
