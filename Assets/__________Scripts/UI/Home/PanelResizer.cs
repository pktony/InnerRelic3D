using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelResizer : MonoBehaviour
{
    private RectTransform panelRect;

    private void Awake()
    {
        panelRect = GetComponent<RectTransform>();   
    }

    //TEMP
    private void Update()
    {
        
    }
}
