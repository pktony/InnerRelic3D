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
        notificationText = DataManager.Inst.textManager.GetStringData("restart_button");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        SceneManager.LoadScene("Home");
    }
}
