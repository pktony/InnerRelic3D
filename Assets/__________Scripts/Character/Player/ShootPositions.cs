using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class ShootPositions : MonoBehaviour
{
    private Transform[] shootPositions;

    public float shootAngle = 30f;

    private void Awake()
    {
        shootPositions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            shootPositions[i] = transform.GetChild(i);
    }

    public Transform[] InitializeShootPosition(int shootCount)
    {
        for(int i = 0; i < transform.childCount; i ++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        if (shootCount == 1)
        {
            shootPositions[0].gameObject.SetActive(true);
            shootPositions[0].localRotation = Quaternion.identity;
        }
        else
        {
            float anglePerArrow = shootAngle / shootCount;
            for (int i = 0; i < shootCount; i++)
            {
                shootPositions[i].gameObject.SetActive(true);
                shootPositions[i].localRotation = Quaternion.Euler(0f, -shootAngle * 0.5f + anglePerArrow * i, 0f);
            }
        }

        return shootPositions;
    }
}
