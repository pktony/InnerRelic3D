using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

public class ShootPositions : MonoBehaviour
{
    private Transform[] shootPositions;

    public float shootAngle = 30f;
    public GameObject shootPositionPrefab;

    public void InitailizeShootPosistions(int maxShootCount)
    {
        for (int i = 0; i < maxShootCount; i++)
        {
            GameObject obj = Instantiate(shootPositionPrefab, transform);
            obj.SetActive(false);
        }

        shootPositions = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            shootPositions[i] = transform.GetChild(i);
    }

    public Transform[] ActivateShootPositions(int shootCount)
    {
        for(int i = 0; i < transform.childCount; i ++)
        {// 먼저 전부 끄고
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
