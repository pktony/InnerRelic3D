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

public enum EnemyState : byte
{
    Idle = 0,
    Patrol,
    Track,
    Attack,
    Knockback,
    Die,
}
