using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerState : byte
{
    Walk = 0,
    Run
}
public enum ControlMode : byte 
{
    Normal = 0, 
    AimMode,
    LockedOn 
}

public enum Weapons : byte
{
    Sword = 0,
    Bow,
}

public enum Skills : byte
{
    Heal,
    SpecialAttack_Sword,
    SpecialAttack_Archer,
    Defence,
    Dodge,
    SkillCount
}

public enum EnemyState : byte
{
    Idle = 0,
    Track,
    Attack,
    Knockback,
    Die,
}

public enum UIWindow : byte
{
    Home = 0,
    Setting,
    LeaderBoard
}
