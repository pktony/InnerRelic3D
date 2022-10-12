using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Utility;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    Weapons weaponIndex = Weapons.Bow;

    private GameObject[] warriorType;
    private PlayerController[] controllers;
    private CoolTimeUIManager coolTimeUIs;

    private CinemachineVirtualCamera virtualCam;
    private Vector3 originalPosition;
    private Vector3 movePos;

    private ParticleSystem switchParticle;

    private void Awake()
    {
        warriorType = new GameObject[2];
        warriorType[0] = transform.GetChild(0).gameObject;
        warriorType[1] = transform.GetChild(1).gameObject;

        controllers = new PlayerController[2];
        controllers[0] = GetComponentInChildren<PlayerController_Sword>();
        controllers[1] = GetComponentInChildren<PlayerController_Archer>();

        coolTimeUIs = FindObjectOfType<CoolTimeUIManager>();
        virtualCam = FindObjectOfType<CinemachineVirtualCamera>();

        switchParticle = transform.GetChild(3).GetComponent<ParticleSystem>();
    }

    private void Start()
    {
        SwitchWeapon(Weapons.Sword);
    }


    /// <summary>
    /// 2가지 캐릭터를 번갈아 가면서 껐다 켜는 기능
    /// Hierarchy 순서에 따라 변경
    /// </summary>
    /// <param name="weaponType"> 변경할 무기 </param>
    public void SwitchWeapon(Weapons weaponType)
    {
        if (weaponIndex != weaponType)
        {// 다를 때만 실행 
            //스킬 UI 관련
            switchParticle.Play();
            coolTimeUIs[2 - (int)weaponType].gameObject.SetActive(false);   // HP UI
            coolTimeUIs[3 - (int)weaponType].gameObject.SetActive(false);   // Special Attack
            coolTimeUIs[4 - (int)weaponType].gameObject.SetActive(false);   // Defence / Dodge UI

            coolTimeUIs[1 + (int)weaponType].gameObject.SetActive(true);    // HP UI
            coolTimeUIs[2 + (int)weaponType].gameObject.SetActive(true);    // Special Attack
            coolTimeUIs[3 + (int)weaponType].gameObject.SetActive(true);    // Defence / Dodge UI

            // 캐릭터 변경
            warriorType[(int)weaponType].SetActive(true);
            warriorType[1 - (int)weaponType].SetActive(false);
            weaponIndex = weaponType;

            // 현재 컨트롤 모드 동기화
            if (weaponType == Weapons.Sword)
            {
                controllers[0].ControlMode = controllers[1].ControlMode;
            }
            else
            {
                controllers[1].ControlMode = controllers[0].ControlMode;
            }

            // 0 선택 => 1 true, 2 false
            // 1 선택 => 2 true, 1 false
        }
    }
    
}