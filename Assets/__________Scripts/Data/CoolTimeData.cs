using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolTimeData
{
    private SkillData data;

    protected float coolTime;
    protected float currentCoolTime = 0f;

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

    public CoolTimeData(SkillData _data)
    {
        data = _data;
        coolTime = data.coolTime;
        currentCoolTime = 0f;
    }

    public bool IsReadyToUse() => currentCoolTime <= 0f;

    public void ResetCoolTime() => currentCoolTime = coolTime;
}
