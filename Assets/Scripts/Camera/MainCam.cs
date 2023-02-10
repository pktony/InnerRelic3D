using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    public CinemachineVirtualCamera mainCamVirtual { get; private set; }

    public void InitializeMainCam()
    {
        mainCamVirtual = GetComponent<CinemachineVirtualCamera>();
        mainCamVirtual.Follow = GameManager.Inst.Player_Stats.transform;
        mainCamVirtual.LookAt = GameManager.Inst.Player_Stats.transform;
    }

    public void SetTarget(Transform target)
    {
        mainCamVirtual.LookAt = target;
    }
}
