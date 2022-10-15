using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelpButton : MonoBehaviour, IPointerClickHandler
{
    CanvasGroup mainGroup;

    private void Awake()
    {
        mainGroup = GetComponentInParent<CanvasGroup>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        
    }
}
