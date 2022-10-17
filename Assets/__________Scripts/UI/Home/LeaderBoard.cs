using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    LeaderBoardLine[] ranks;
    private const int rankCount = 6;

    private void Awake()
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
}
