using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    // 쿨타임

    private float coolTime;
    private float currentCoolTime = 0f;

    public System.Action<float, float> onCoolTimeChange;

    public float CurrentCoolTime
    {
        get => currentCoolTime;
        set
        {
            currentCoolTime = value;
            onCoolTimeChange?.Invoke(coolTime, currentCoolTime );
        }
    }

    public SkillData(float _coolTime)
    {
        coolTime = _coolTime;
        currentCoolTime = _coolTime;
    }


    public void ResetCoolTime() => currentCoolTime = coolTime;
}
