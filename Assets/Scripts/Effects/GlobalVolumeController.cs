using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    Volume volume;

    ColorAdjustments colorChanger;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet<ColorAdjustments>(out colorChanger);
    }

    /// <summary>
    /// 게임 오버 시 화면을 흑백으로 전환해주는 함수 
    /// </summary>
    public void ChangeSaturation()
    {
        colorChanger.saturation.overrideState = true;
        StartCoroutine(MakeGray());
    }

    private IEnumerator MakeGray()
    {
        float value = colorChanger.saturation.value;
        while (value > -100f)
        {
            value -= 100f * Time.deltaTime;
            colorChanger.saturation.value = value;
            yield return null;
        }
    }    
}
