using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeButton : Button_Base
{
    private void Start()
    {
        notificationText = "Return to Home";
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        SceneManager.LoadScene("Home");
    }
}
