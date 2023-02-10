using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LockonCam : MonoBehaviour
{
    public CinemachineVirtualCamera lockonCamVirtual { get; private set; }

    public void InitializeLockonCam()
    {
        lockonCamVirtual = GetComponent<CinemachineVirtualCamera>();
        lockonCamVirtual.Follow = GameManager.Inst.Player_Stats.lockonFollowTarget;
    }


    public void SetTarget(Transform target)
    {
        lockonCamVirtual.LookAt = target;
    }

}
