using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeaderBoardButtons : MonoBehaviour, IPointerClickHandler
{
    UIManager uiManager;
    SettingManager settingManager;

    HomeButtons homeButtons;

    private void Awake()
    {
        homeButtons = FindObjectOfType<HomeButtons>();
    }

    private void Start()
    {
        uiManager = UIManager.Inst;
        settingManager = SettingManager.Inst;
    }

    public void OnPointerClick(PointerEventData _)
    {
        uiManager.LeaderBoard_Home.ShowLeaderBoard();
        homeButtons.HideButtons();
        settingManager.Panel.SetWindowSize(UIWindow.LeaderBoard);
        SoundManager.Inst.PlaySound_UI(UIClips.Click);
    }
}
