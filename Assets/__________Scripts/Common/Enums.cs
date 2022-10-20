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

public enum MusicClips : byte
{
    Home = 0,
    RoundStart,
    Gameover,
}

public enum UIClips : byte
{
    Click = 0,
    Drag,
    InfoPop,
    Victory,
    TimeTicking,
}

public enum PlayerClips : byte
{
    NormalAttack_Sword1 = 0,
    NoramlAttack_Sword2,
    NoramlAttack_Sword3,
    NoramlAttack_Archer,
    SpecialAttack_Sword,
    SpecialAttack_Demacia,
    SpecialAttack_Arhcer,
    Defence,
    Dodge,
    Lockon,
    Lockoff,
    LockonFailed,
    Hit1,
    Hit2,
    Hit3,
    Hit4,
    Jump,
    DoubleJump,
    Heal,
}

public enum EnemyClip : byte
{
}
