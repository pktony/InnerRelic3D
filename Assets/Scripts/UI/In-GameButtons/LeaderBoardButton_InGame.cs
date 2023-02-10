using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeaderBoardButton_InGame : Button_Base
{
    UIManager uiManager;
    protected override void Awake()
    {
        base.Awake();
    }
    void Start()
    {
        uiManager = UIManager.Inst;
        notificationText = DataManager.Inst.textManager.GetStringData("Leaderboard");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        uiManager.LeaderBoard_InGame.ShowLeaderBoard();
    }
}
