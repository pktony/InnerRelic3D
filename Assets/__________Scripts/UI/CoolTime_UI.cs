using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CoolTime_UI : MonoBehaviour
{
    Image buttonImg;

    private void Awake()
    {
        buttonImg = transform.GetChild(0).GetComponent<Image>();
    }

    public void RefreshUI(float coolTime, float currentCoolTime)
    {
        buttonImg.fillAmount = currentCoolTime / coolTime;
    }
}
