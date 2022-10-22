using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
