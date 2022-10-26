using Cinemachine;
using UnityEngine;

/// <summary>
/// 락온 됐을 때 플레이어가 항상 카메라 앞에 있도록 설정해주는 클래스 
/// </summary>
public class LockOnFollowTarget : MonoBehaviour
{
    PlayerStats stats;
    public CinemachineVirtualCamera lockonCam;

    public Vector3 offset;

    private void Start()
    {
        stats = GameManager.Inst.Player_Stats;
    }

    private void Update()
    {
        if(lockonCam.isActiveAndEnabled)
            transform.localPosition = stats.GetTargetDirection() + offset;
    }
}
