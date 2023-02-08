using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard_InGame : LeaderBoard_Base
{
    protected override void Awake()
    {
        base.Awake();
        group = GetComponentInParent<CanvasGroup>();
    }
}
