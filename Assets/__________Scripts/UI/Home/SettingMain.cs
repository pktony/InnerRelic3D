using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingMain : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Image volumeBar;    // EventSystem용 Image
    Image masterVolumeBar;
    Image musicVolumeBar;

    HomeButtons homeButtons;

    GraphicRaycaster raycaster;
    List<RaycastResult> rayResults = new(3);

    float leftEndPosition;
    float imageWidth;


    private void Awake()
    {
        homeButtons = GetComponentInParent<HomeButtons>();
        raycaster = GetComponentInParent<GraphicRaycaster>();

        Transform barParent = transform.GetChild(0);
        masterVolumeBar = barParent.GetChild(1).GetChild(0).GetChild(0).GetComponent<Image>();
        musicVolumeBar = barParent.GetChild(2).GetChild(0).GetChild(0).GetComponent<Image>();
    }

    private void Start()
    {// 처음 시작 초기화 
        SettingData data = SettingManager.Inst.LoadSettingValues();
        masterVolumeBar.fillAmount = data.masterVolume;
        musicVolumeBar.fillAmount = data.musicVolume;
    }

    private void CheckImage(PointerEventData eventData)
    {
        if (rayResults[0].gameObject.TryGetComponent<Image>(out volumeBar))
        {// Image 컴포넌트 찾기
            if (!volumeBar.CompareTag("MasterVolume") || !volumeBar.CompareTag("MusicVolume"))
            {
                if(volumeBar.TryGetComponent<BackButton>(out BackButton exit))
                {// Home으로 나가기 
                    exit.HideSettings();
                    homeButtons.ShowButtons();
                    SettingManager.Inst.Panel.SetWindowSize(UIWindow.Home);
                    SettingManager.Inst.SaveSettingValues();
                    return;
                }
            }
            if (volumeBar.TryGetComponent<RectTransform>(out RectTransform rect))
            {// 왼쪽 끝을 찾기 위한 RectTransform
                imageWidth = rect.sizeDelta.x;
                leftEndPosition = rayResults[0].gameObject.transform.position.x - imageWidth * 0.5f;
                volumeBar.fillAmount = (eventData.position.x - leftEndPosition) / imageWidth;
            }
        }
    }

    private void AdjustVolume()
    {
        if (volumeBar.CompareTag("MasterVolume"))
        {
            SettingManager.Inst.MasterVol = volumeBar.fillAmount;
        }
        else if (volumeBar.CompareTag("MusicVolume"))
        {
            SettingManager.Inst.MusicVolume = volumeBar.fillAmount;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        raycaster.Raycast(eventData, rayResults);
        CheckImage(eventData);
        rayResults.Clear();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (volumeBar != null)
        {
            volumeBar.fillAmount = volumeBar.fillAmount = (eventData.position.x - leftEndPosition) / imageWidth;
            AdjustVolume();
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        volumeBar = null;
    }

    /// <summary>
    /// 클릭을 하면 Setting Bar 또는 Icon이 레이케스트 된다
    /// SettingBar의 왼쪽 끝 위치 기준으로 얼마나 떨어진 곳에서 클릭 됐는지 확인해야 함  
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        raycaster.Raycast(eventData, rayResults);

        CheckImage(eventData);
        AdjustVolume();

        rayResults.Clear();
    }
}
