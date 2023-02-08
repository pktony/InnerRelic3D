using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolTimeData
{
    private SkillData data;

    private readonly float coolTime;
    private float currentCoolTime = 0f;

    public System.Action<float, float> onCoolTimeChange;    // <스킬 쿨타임, 현재 쿨타임>

    public float CurrentCoolTime
    {
        get => currentCoolTime;
        set
        {
            currentCoolTime = value;
            onCoolTimeChange?.Invoke(coolTime, currentCoolTime );
        }
    }

    /// <summary>
    /// 쿨타임 데이터 생성자 
    /// </summary>
    /// <param name="_data"></param>
    public CoolTimeData(SkillData _data)
    {
        data = _data;
        coolTime = data.coolTime;
        currentCoolTime = 0f;
    }

    /// <summary>
    /// 현재 쿨타임이 0 이하일 때 사용가능 
    /// </summary>
    /// <returns></returns>
    public bool IsReadyToUse() => currentCoolTime <= 0f;

    /// <summary>
    /// 스킬 사용 시 스킬 쿨 초기화 함수 
    /// </summary>
    public void ResetCoolTime() => currentCoolTime = coolTime;
}
