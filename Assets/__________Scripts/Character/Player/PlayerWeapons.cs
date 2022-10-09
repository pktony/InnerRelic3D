using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    // 무기 변경
    //  - 무기 종류 : 검 / 활  -> Scriptable Object
    //  - Scribtable Object : 데미지 관련 수치 및 공격 함수
    //  - 숫자 키 1 2로 무기 변경

    // 무기를 바꾸는 순간 애니메이터도 바꿔줘야 한다

    private GameObject[] warriorType;
    private PlayerController[] controllers;

    private void Awake()
    {
        warriorType = new GameObject[2];
        warriorType[0] = transform.GetChild(0).gameObject;
        warriorType[1] = transform.GetChild(1).gameObject;
        controllers = GetComponents<PlayerController>();
    }

    private void Start()
    {
        SwitchWeapon(Weapons.Sword);
    }

    public void SwitchWeapon(Weapons weaponType)
    {
        controllers[1 - (int)weaponType].enabled = false;
        warriorType[1 - (int)weaponType].SetActive(false);

        controllers[(int)weaponType].enabled = true;
        warriorType[(int)weaponType].SetActive(true);
    }
}