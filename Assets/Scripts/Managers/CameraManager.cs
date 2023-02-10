using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private MainCam mainCam;
    private LockonCam lockonCam;

    private void Start()
    {
        InitializeCameras();
    }

    private void InitializeCameras()
    {
        mainCam = FindObjectOfType<MainCam>();
        lockonCam = FindObjectOfType<LockonCam>();

        mainCam.InitializeMainCam();
        lockonCam.InitializeLockonCam();

        mainCam.gameObject.SetActive(false);
        lockonCam.gameObject.SetActive(false);
    }


    public void Lockon(Transform target)
    {
        mainCam.gameObject.SetActive(false);
        lockonCam.SetTarget(target);
        lockonCam.gameObject.SetActive(true);
    }

    public void LockOff()
    {
        lockonCam.SetTarget(null);
        lockonCam.gameObject.SetActive(false);
        mainCam.transform.position = lockonCam.transform.position;
        mainCam.gameObject.SetActive(true);
    }
}
