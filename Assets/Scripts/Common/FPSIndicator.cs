using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// FPS를 표시하는 클래스 
/// </summary>
public class FPSIndicator : MonoBehaviour
{
    private TextMeshProUGUI fpsText;
    private WaitForSeconds waitSeconds;

    private float fps = 0f;
    private float deltaTime = 0f;
    private readonly float updateInterval = 1.0f;

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

    private IEnumerator RefreshFPS()
    {// FPS 새로고침 함수 
        while(true)
        {
            fps = 1 / deltaTime;
            fpsText.text = $"{fps:F0} FPS";
            yield return waitSeconds;
        }
    }
}
