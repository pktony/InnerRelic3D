using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 버튼을 누르거나 적이 일정 수 이하 남았을 때 알려주는 기능
///
///  - 알람을 보여주고, 터치나 클릭으로 방해하지 않으면 숨긴다.
///  - 아래로 스와이프 하면 바로 숨겨진다
/// </summary>
public class InfoPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
{
    RectTransform rect;
    TextMeshProUGUI text;

    private float panelHeight;
    private float showSpeed = 5f;
    private float swipeSensitivity = 2f;

    // Flags --------------------------------------------------------------- 
    private bool isInterrupted = false;
    private bool isShowing = false;

    private Vector2 dragOffset;
    private const float ERROR_CORRECTION_NUM = 1.5f;

    Vector2 initialPos;
    Vector2 destinationPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        text = GetComponentInChildren<TextMeshProUGUI>();

        panelHeight = rect.sizeDelta.y;

        initialPos = rect.anchoredPosition;
        destinationPos = initialPos + Vector2.up * panelHeight;
    }


    public void ShowPanel(string notificationText)
    {
        text.text = notificationText;
        if(!isShowing)
            StartCoroutine(RevealPanel());
    }

    private IEnumerator RevealPanel()
    {
        isShowing = true;
        Vector2 dir = initialPos;
        while (rect.anchoredPosition.y < panelHeight - ERROR_CORRECTION_NUM)
        {
            dir = Vector2.Lerp(dir, destinationPos, showSpeed * Time.unscaledDeltaTime);
            rect.anchoredPosition = dir;
            yield return null;
        }
        if (!isInterrupted)
        {
            StartCoroutine(HidePanel(destinationPos, 2.0f));
        }
    }

    /// <summary>
    /// 패널 숨기기 
    /// </summary>
    /// <param name="startAnchoredPos"> 숨겨지기 시작하는 위치 </param>
    /// <param name="waitTime"> 숨겨질때까지 걸리는 시간. Default : 즉시 </param>
    /// <returns></returns>
    private IEnumerator HidePanel(Vector2 startAnchoredPos, float waitTime = 0f)
    {
        yield return new WaitForSecondsRealtime(waitTime);
        Vector2 dir = startAnchoredPos;
        while (rect.anchoredPosition.y > 0f + ERROR_CORRECTION_NUM)
        {
            dir = Vector2.Lerp(dir, initialPos, showSpeed * Time.unscaledDeltaTime);
            rect.anchoredPosition = dir;
            yield return null;
        }
        isShowing = false;
        Debug.Log("숨기기 끝");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isInterrupted = true;
        Debug.Log("Interrupted");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isInterrupted = false;
        StartCoroutine(HidePanel(rect.anchoredPosition, 2.0f));
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        mousePos.x = 0;  // x 값은 항상 고정이어야 한다 
        mousePos.y = Mathf.Clamp(mousePos.y, 0f, panelHeight); // y 값은 움직임 범위 내로 제한
        rect.anchoredPosition = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(eventData.delta.y < - swipeSensitivity )
        {
            StartCoroutine(HidePanel(rect.anchoredPosition));
        }
    }

}
