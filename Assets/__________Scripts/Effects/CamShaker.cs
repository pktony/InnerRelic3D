using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CamShaker : MonoBehaviour
{
    CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    /// <summary>
    /// Cinemachine Impulse로 카메라를 흔드는 함수
    /// </summary>
    /// <param name="shakeTime">Shake 지속시간 </param>
    /// <param name="shakePower">Shake 정도 </param>
    /// <param name="impulseType">Recoil, Bump, Explosion, Rumble</param>
    public void ShakeCamera(float shakeTime, float shakePower,
        CinemachineImpulseDefinition.ImpulseShapes impulseType = CinemachineImpulseDefinition.ImpulseShapes.Recoil)
    {
        impulseSource.m_DefaultVelocity = Random.insideUnitSphere;
        impulseSource.m_ImpulseDefinition.m_ImpulseShape = impulseType; 
        impulseSource.m_ImpulseDefinition.m_ImpulseDuration = shakeTime;
        impulseSource.GenerateImpulse(shakePower);
    }

}
