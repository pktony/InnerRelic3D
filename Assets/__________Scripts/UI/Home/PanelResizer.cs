using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 어떤 버튼을 눌렀는가에 따라서 패널 크기를 조절
///  - 가로 값은 고정시켜 놓고 y 값만 변경
/// 필요한 정보
///  - Buttons의 높이 
///  - Content -> setting의 높이 
///  
/// </summary>
public class PanelResizer : MonoBehaviour
{
    private RectTransform panelRect;
    private Vector2 buttonsSize;
    private Vector2 settingSize;

    private const float WINDOWSIZE_ERROR = 50f;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();

        buttonsSize = transform.parent.GetChild(1).GetComponent<RectTransform>().sizeDelta;
        settingSize = transform.parent.GetChild(2).GetChild(0).GetComponent<RectTransform>().sizeDelta;
    }

    public void SetWindowSize(UIWindow windowType)
    {
        switch (windowType)
        {
            case UIWindow.Home:
                StartCoroutine(AdjustWindowSize_X(buttonsSize));
                StartCoroutine(AdjustWindowSize_Y(buttonsSize));
                break;
            case UIWindow.Setting:
                StartCoroutine(AdjustWindowSize_X(settingSize));
                StartCoroutine(AdjustWindowSize_Y(settingSize));
                break;
            default:
                break;  
        }
    }



    IEnumerator AdjustWindowSize_X(Vector2 goalSize)
    {
        int multiplier_x = 1;
        // 가로 사이즈 
        if(panelRect.sizeDelta.x - goalSize.x > 0)
        {// 현재 패널이 더 크다 -> 줄여야 한다 
            multiplier_x = -1;
        }
        else
        {// 현재 패널이 더 작거나 같다 -> 늘려야 한다 
            multiplier_x = 1;
        }

        while (Mathf.Abs(panelRect.sizeDelta.x - goalSize.x) > WINDOWSIZE_ERROR)
        {
            panelRect.sizeDelta += 500f * Time.deltaTime * multiplier_x * Vector2.right;
            yield return null;
        }
        Debug.Log("X done");
        //StartCoroutine(AdjustWindowSize_Y(goalSize));
    }


    IEnumerator AdjustWindowSize_Y(Vector2 goalSize)
    {
        int multiplier_y = 1;
        // 세로 사이즈
        if (panelRect.sizeDelta.y - goalSize.y > 0)
        {// 현재 패널이 더 크다 
            multiplier_y = -1;
        }
        else
        {// 현재 패널이 더 작거나 같다 
            multiplier_y = 1;
        }

        while (Mathf.Abs(panelRect.sizeDelta.y - goalSize.y) > WINDOWSIZE_ERROR)
        {
            panelRect.sizeDelta += 1500f * Time.deltaTime * multiplier_y * Vector2.up;
            yield return null;
        }
        Debug.Log("Y Done");
    }
}
