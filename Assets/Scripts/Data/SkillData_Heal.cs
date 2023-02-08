using UnityEngine;

/// <summary>
/// 힐 전용 스킬 데이터 Scriptable Object
/// </summary>
[CreateAssetMenu(fileName = "New Skill_Heal", menuName = "Scriptable Object/Skill Data_Heal", order = 2)  ]
public class SkillData_Heal : SkillData
{
    public float healInterval = 1.0f;
    public float healAmount = 15f;
    public int healTickNum = 5;
}
