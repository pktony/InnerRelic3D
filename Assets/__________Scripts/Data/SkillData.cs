using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Scriptable Object/Skill Data", order = 1)  ]
public class SkillData : ScriptableObject
{
    public float coolTime = 5f;
    public GameObject[] skillParticles; 
}
