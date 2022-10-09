using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_Sword : PlayerController
{
    protected override void OnAttack(InputAction.CallbackContext _)
    {
        anim.SetTrigger("onAttack");
        anim.SetFloat("ComboTimer", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
    }

    protected override void OnRightClick(InputAction.CallbackContext obj)
    {// 방어
    }


#if UNITY_EDITOR
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
#endif
}
