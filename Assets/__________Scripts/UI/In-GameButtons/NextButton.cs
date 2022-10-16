using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public class NextButton : Button_Base  
{
    private void Start()
    {
        notificationText = "To the Next Round";
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        // 다음 라운드 시작
        GameManager.Inst.RoundStart();
        base.OnPointerUp(eventData);
    }
}
