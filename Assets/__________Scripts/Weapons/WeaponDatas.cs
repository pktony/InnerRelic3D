using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapons Data", menuName = "Scriptable Object/Weapon Datas", order = 0)]
public class WeaponDatas : ScriptableObject
{
    public GameObject weaponPrefab;
    public Transform weaponParent;

    public float attackPower = 1.0f;
}
