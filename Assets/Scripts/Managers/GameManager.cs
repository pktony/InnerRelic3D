using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Inst => instance;

    Player mainPlayer;
    public Player MainPlayer => mainPlayer;

    #region TEMPORARY
    Enemy dog;
    public Enemy Dog => dog;
    #endregion

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            instance.Initiailize();
            DontDestroyOnLoad(this.gameObject);

        }
        else
        {
            if (instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void Initiailize()
    {
        mainPlayer = FindObjectOfType<Player>();

        #region TEMPORARY
        dog = FindObjectOfType<Enemy>();
        #endregion
    }
}
