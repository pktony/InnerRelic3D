using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeButtons : MonoBehaviour
{
    Image playImage;
    Image settingImage;
    Image leaderBoardImage;

    private void Awake()
    {
        Transform buttonParent = transform.GetChild(1);
        playImage = buttonParent.GetChild(0).GetComponent<Image>();
        settingImage = buttonParent.GetChild(1).GetComponent<Image>();
        leaderBoardImage = buttonParent.GetChild(2).GetComponent<Image>();
    }

    public void HideButtons()
    {
        TurnoffButton(playImage);
        TurnoffButton(settingImage);
        TurnoffButton(leaderBoardImage);
    }

    public void ShowButtons()
    {
        TurnOnButton(playImage);
        TurnOnButton(settingImage);
        TurnOnButton(leaderBoardImage);
    }

    private void TurnoffButton(Image image)
    {
        image.color = Color.clear;
        image.raycastTarget = false;
    }

    private void TurnOnButton(Image image)
    {
        image.color = Color.white;
        image.raycastTarget = true;
    }
}
