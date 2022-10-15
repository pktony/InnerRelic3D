using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamDrag_Panel : MonoBehaviour, IDragHandler
{
    CinemachineVirtualCamera mainCam_Virtual;
    CinemachineOrbitalTransposer orbitalTransposer;

    public float maxTurnSpeed = 20f;
    private float maxTurn_Y = 8f;

    private float sensitivity_X = 1f;
    private float sensitivity_Y = 0.1f;


    private void Awake()
    {
        mainCam_Virtual = FindObjectOfType<MainCam>(true).GetComponent<CinemachineVirtualCamera>();
        orbitalTransposer = mainCam_Virtual.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            eventData.delta = Vector2.ClampMagnitude(eventData.delta, maxTurnSpeed);
            orbitalTransposer.m_XAxis.Value += eventData.delta.x * sensitivity_X;
            orbitalTransposer.m_FollowOffset.y =
                Mathf.Clamp(orbitalTransposer.m_FollowOffset.y - eventData.delta.y * sensitivity_Y, 0f, maxTurn_Y);
        }
    }

}
