using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingContents : MonoBehaviour
{
    private RectTransform contentRect;

    private void Awake()
    {
        contentRect = GetComponent<RectTransform>();
    }
}
