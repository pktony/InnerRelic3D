using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class LockOnFollowTarget : MonoBehaviour
{
    public CinemachineVirtualCamera lockonCam;    

    public Vector3 offset;

    private void Update()
    {
        if(lockonCam.isActiveAndEnabled)
            transform.localPosition = GameManager.Inst.Player_Stats.GetTargetDirection() + offset;
    }

}
