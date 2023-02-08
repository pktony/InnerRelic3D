using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노치나 펀치홀 디스플레이 UI 짤림을 방지하기 위한 Canvas Safe Area 반영 
/// </summary>
public class SafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    Rect safeArea;

    Vector2 minAnchor = Vector2.zero;  // safe area의 왼쪽 아래 
    Vector2 maxAnchor = Vector2.zero;  // safe area의 오른쪽 위 

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        safeArea = Screen.safeArea;

        minAnchor = safeArea.position;
        maxAnchor = minAnchor + safeArea.size;

        // 스크린 사이즈 기준으로 safe area까지 비율 설정
        minAnchor.x /= UnityEngine.Device.Screen.width;
        maxAnchor.x /= UnityEngine.Device.Screen.width;
        minAnchor.y /= UnityEngine.Device.Screen.height;
        maxAnchor.y /= UnityEngine.Device.Screen.height;

        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
}
