using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SettingButton : MonoBehaviour, IPointerClickHandler
{
    CanvasGroup contentGroup;
    HomeButtons homeButtons;

    private void Awake()
    {
        contentGroup = transform.parent.parent.GetChild(2).GetChild(0).GetComponent<CanvasGroup>();
        homeButtons = GetComponentInParent<HomeButtons>();
    }

    public void OnPointerClick(PointerEventData _)
    {
        contentGroup.alpha = 1.0f;
        contentGroup.interactable = true;
        contentGroup.blocksRaycasts = true;
        homeButtons.HideButtons();
        SettingManager.Inst.Panel.SetWindowSize(UIWindow.Setting);
    }

}
