using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill_Heal", menuName = "Scriptable Object/Skill Data_Heal", order = 2)  ]
public class SkillData_Heal : SkillData
{
    public float healInterval = 1.0f;
    public float healAmount = 15f;
    public int healTickNum = 5;
}
