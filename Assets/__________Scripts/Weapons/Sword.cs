using UnityEngine;

/// <summary>
/// 소드의 콜라이더를 직접 조종하기 위한 클래스 
/// </summary>
public class Sword : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent<IBattle>(out var target))
            {// Enemy의 Ibattle
                GameManager.Inst.Player_Stats.Attack(target);
            }
        }
    }
}
