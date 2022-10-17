using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeaderBoardButtons : MonoBehaviour, IPointerClickHandler
{
    CanvasGroup boardGroup;
    HomeButtons homeButtons;

    private void Awake()
    {
        boardGroup = transform.parent.parent.GetChild(2).GetChild(1).GetComponent<CanvasGroup>();
        homeButtons = FindObjectOfType<HomeButtons>();
    }

    public void OnPointerClick(PointerEventData _)
    {
        boardGroup.alpha = 1f;
        boardGroup.interactable = true;
        boardGroup.blocksRaycasts = true;
        homeButtons.HideButtons();
        SettingManager.Inst.Panel.SetWindowSize(UIWindow.LeaderBoard);
    }
}
