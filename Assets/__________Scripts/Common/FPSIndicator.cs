using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSIndicator : MonoBehaviour
{
    TextMeshProUGUI fpsText;
    WaitForSeconds waitSeconds;

    float fps = 0f;
    float deltaTime = 0f;
    float updateInterval = 1.0f;

    private void Awake()
    {
        fpsText = GetComponent<TextMeshProUGUI>();
        waitSeconds = new WaitForSeconds(updateInterval);
    }

    private void Start()
    {
        StartCoroutine(RefreshFPS());
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    IEnumerator RefreshFPS()
    {// FPS 새로고침 함수 
        while(true)
        {
            fps = 1 / deltaTime;
            fpsText.text = $"{fps:F0} FPS";
            yield return waitSeconds;
        }
    }
}
