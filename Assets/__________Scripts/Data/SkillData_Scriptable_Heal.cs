using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill_Heal", menuName = "Scriptable Object/Skill Data_Heal", order = 2)  ]
public class SkillData_Scriptable_Heal : ScriptableObject
{
    public float coolTime = 10f;

    public float healInterval = 1.0f;
    public float healAmount = 15f;
    public int healTickNum = 5;

    public GameObject[] skillParticles;
    // [0] Use
    // [1] Per Tick
}
