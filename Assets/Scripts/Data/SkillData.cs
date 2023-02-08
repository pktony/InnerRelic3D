using UnityEngine;

/// <summary>
/// 스킬 데이터 Scriptable Object
/// </summary>
[CreateAssetMenu(fileName = "New Skill", menuName = "Scriptable Object/Skill Data", order = 1)  ]
public class SkillData : ScriptableObject
{
    public float coolTime = 5f;
    public GameObject[] skillParticles; 
}
