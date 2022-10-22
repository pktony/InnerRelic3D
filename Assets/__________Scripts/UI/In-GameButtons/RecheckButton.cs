using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RecheckButton : MonoBehaviour
{
    CanvasGroup canvasGroup;
    Button yesButton;
    Button noButton;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        yesButton = transform.GetChild(0).GetComponent<Button>();
        noButton = transform.GetChild(1).GetComponent<Button>();

        yesButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Stage");
            Time.timeScale = 1.0f;
        });
    
        noButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        });
    }
}
