using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    PlayerStats mainPlayer;
    public PlayerStats Player_Stats => mainPlayer;

    PlayerController_Archer archerController;
    public PlayerController_Archer ArcherController => archerController;


    protected override void Initialize()
    {
        mainPlayer = FindObjectOfType<PlayerStats>();
        archerController = FindObjectOfType<PlayerController_Archer>();

        
    }
}
