using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RestartButton : Button_Base
{
    private CanvasGroup recheckGroup;

    protected override void Awake()
    {
        base.Awake();
        recheckGroup = transform.parent.GetChild(2).GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        notificationText = "*** RESTART THE GAME ***";
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        Time.timeScale = 0f;

        recheckGroup.alpha = 1.0f;
        recheckGroup.interactable = true;
        recheckGroup.blocksRaycasts = true;
    }

}
