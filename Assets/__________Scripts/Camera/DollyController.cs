using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

/// <summary>
/// 홈, 인트로에서 돌리 카메라를 조종하는 클래스 
/// </summary>
public class DollyController : MonoBehaviour
{
    CinemachineDollyCart dollyCart;
    GameObject dollyCam;
    MainCam mainCam;

    CamDrag_Panel controlUI;

    public Action onIntroEnd;

    private void Awake()
    {
        dollyCart = GetComponent<CinemachineDollyCart>();
        dollyCam = GameObject.Find("DollyCam");

        mainCam = FindObjectOfType<MainCam>(true);

        controlUI = FindObjectOfType<CamDrag_Panel>();
    }

    /// <summary>
    /// 인게임 인트로 초기화 함수 
    /// </summary>
    public void InitializeIntroUIs()
    {
        dollyCart.m_Position = 0f;
        controlUI.transform.parent.gameObject.SetActive(false);

        StartCoroutine(Intro());
    }

    private IEnumerator Intro()
    {
        while (true)
        {
            if (dollyCart.m_Position >= dollyCart.m_Path.PathLength)
            {
                dollyCam.SetActive(false);
                mainCam.gameObject.SetActive(true);
                GameManager.Inst.Player_Stats.gameObject.SetActive(true);
                controlUI.transform.parent.gameObject.SetActive(true);
                UIManager.Inst.RoundUI.gameObject.SetActive(true);

                onIntroEnd?.Invoke();
                break;
            }
            yield return null;
        }
    }
}
