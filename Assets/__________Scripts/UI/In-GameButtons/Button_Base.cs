using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Button_Base : MonoBehaviour, IPointerEnterHandler, IPointerUpHandler
{
    protected CanvasGroup buttonGroup;

    protected string notificationText;

    protected virtual void Awake()
    {
        buttonGroup = GetComponentInParent<CanvasGroup>();
    }

    /// <summary>
    /// Notification 패널 보이기 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        UIManager.Inst.InfoPanel.ShowPanel(notificationText);
    }

    /// <summary>
    /// 상호작용 시 버튼 사라지게 처리
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        buttonGroup.alpha = 0f;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
    }
}
