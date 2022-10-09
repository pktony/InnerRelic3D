using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealUI : MonoBehaviour
{
    Image buttonImg;

    private bool isHealing = false;

    // ---- Skills    
    float healCoolTime = 10.0f;
    float timer = 0f;

    // ############### Property
    public bool IsHealing
    {
        get => isHealing;
        set
        {
            isHealing = value;
        }
    }

    private void Awake()
    {
        buttonImg = transform.GetChild(0).GetComponent<Image>();
    }

    private void Update()
    {
        if(isHealing)
        {
            timer += Time.deltaTime;
            buttonImg.fillAmount = timer / healCoolTime;
            if(timer > healCoolTime)
            {
                timer = 0f;
                isHealing = false;
            }
        }
    }
}
