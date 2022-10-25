using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoolTimeManager : MonoBehaviour
{
    CoolTime_UI[] coolTime_UIs;

    public CoolTime_UI this[int index] => coolTime_UIs[index];


    public void InitializeUIs()
    {
        coolTime_UIs = GetComponentsInChildren<CoolTime_UI>();
    }    
}
