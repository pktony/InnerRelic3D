using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard_Base : MonoBehaviour
{
    protected CanvasGroup group;

    LeaderBoardLine[] ranks;
    private const int rankCount = 6;

    protected virtual void Awake()
    {
        ranks = new LeaderBoardLine[rankCount];
        for (int i = 0; i < rankCount; i++)
        {
            ranks[i] = transform.GetChild(0).GetChild(i).GetComponent<LeaderBoardLine>();
        }
    }

    public void RefreshLeaderBoard()
    {
        for(int i = 0; i < rankCount; i++)
        {
            ranks[i].SetNewRank(SettingManager.Inst.Scores[i]);
        }
    }

    public void ShowLeaderBoard()
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void HideLeaderBoard()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
