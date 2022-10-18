using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BackButton_Base : MonoBehaviour, IPointerClickHandler
{
    protected UIManager uiManager;

    protected virtual void Start()
    {
        uiManager = UIManager.Inst;
    }

    protected virtual void HideContent()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HideContent();
    }
}
