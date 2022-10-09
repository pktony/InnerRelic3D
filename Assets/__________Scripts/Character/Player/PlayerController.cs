using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PlayerController : MonoBehaviour
{
    InputActions actions;
    protected Animator anim;
    protected CharacterController controller;
    private CinemachineVirtualCamera virtualCam;


    PlayerState moveMode = PlayerState.Walk;
    [SerializeField] protected ControlMode controlMode = ControlMode.Normal;
    Weapons weaponIndex = Weapons.Sword;

    // ################### Move ######################3
    Vector3 moveDir = Vector3.zero;
    float moveSpeed = 0f;
    Quaternion relativeMoveDir = Quaternion.identity;

    [SerializeField] float walkSpeed = 3.0f;

    // ################### Look ########################
    Vector3 keyboardInputDirection = Vector3.zero;
    Vector2 lookDir = Vector2.zero;
    [SerializeField] float turnSpeed = 15f;

    // ################### Jump ########################
    private bool isGrounded = true;
    private float jumpHeight = 5.0f;
    private uint jumpCounter = 0;
    private Vector3 jumpVector;
    private Vector3 gravity;

    // ################ Target Lockon
    [SerializeField] float lockOnRadius = 20f;
    [SerializeField] GameObject lockOnEffect;
    [SerializeField] GameObject lockOnEffect_Ground;
    protected Transform lockonTarget;
    Collider[] lockonCollider = new Collider[5];

    // ################ Weapon Change
    PlayerWeapons playerWeapon;

    // ################## Properties #####################
    public Vector3 LookDir => lookDir;

    protected virtual void Awake()
    {
        actions = new();
        controller = GetComponent<CharacterController>();
        RefreshAnimator(Weapons.Sword);
        playerWeapon = GetComponent<PlayerWeapons>();

        gravity = Physics.gravity;

        lockOnEffect.SetActive(false);
        lockOnEffect_Ground.SetActive(false);
    }

    private void Start()
    {
        moveSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        actions.Enable();
        actions.Player.Move.performed += OnMoveInput;
        actions.Player.Move.canceled += OnMoveInput;
        actions.Player.Jump.performed += OnJumpInput;
        actions.Player.Attack.performed += OnAttack;
        actions.Player.Attack.canceled += OnAttack;
        actions.Player.RightClick.performed += OnRightClick;
        actions.Player.RightClick.canceled += OnRightClick;
        actions.Player.SpecialAttack.performed += SpecialAttack_Performed;
        actions.Player.SpecialAttack.canceled += SpecialAttack_Performed;
        actions.Player.Heal.performed += Heal_Performed;
        actions.Player.LockOn.performed += OnLockOn;
        actions.Player.WeaponChange.performed += OnWeaponChange;
    }

    private void OnDisable()
    {
        actions.Player.SpecialAttack.performed -= SpecialAttack_Performed;
        actions.Player.SpecialAttack.canceled -= SpecialAttack_Performed;
        actions.Player.Heal.performed -= Heal_Performed;
        actions.Player.WeaponChange.performed -= OnWeaponChange;
        actions.Player.LockOn.performed -= OnLockOn;
        actions.Player.Jump.performed -= OnJumpInput;
        actions.Player.Attack.performed -= OnAttack;
        actions.Player.Attack.canceled -= OnAttack;
        actions.Player.RightClick.performed -= OnRightClick;
        actions.Player.RightClick.canceled -= OnRightClick;
        actions.Player.Move.performed -= OnMoveInput;
        actions.Player.Move.canceled -= OnMoveInput;
        actions.Disable();
    }

    private void Update()
    {
        Move_Turn();

        jumpVector += gravity * Time.deltaTime;
        controller.Move(jumpVector * Time.deltaTime);
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            anim.SetBool("isOnAir", false);
            jumpCounter = 0;
        }
    }

    private void Move_Turn()
    {
        if (keyboardInputDirection.sqrMagnitude > 0f)
        {
            moveDir = keyboardInputDirection;

            switch (controlMode)
            {
                case ControlMode.Normal:
                    // 플레이어가 카메라를 기준으로 방향을 결정한다 
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.AimMode:
                    // 플레이어가 카메라를 앞 방향으로 움직인다 
                    Vector3 cameraForwardProjection = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
                    relativeMoveDir = Quaternion.LookRotation(cameraForwardProjection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed);
                    moveDir = relativeMoveDir * moveDir;
                    break;
                case ControlMode.LockedOn:
                    // 플레이어가 락온된 상대를 바라보며 카메라를 기준으로 움직인
                    if (lockonTarget != null)
                    {
                        relativeMoveDir = Quaternion.LookRotation(lockonTarget.position - transform.position, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed);
                        moveDir = relativeMoveDir * moveDir;
                    }
                    break;
            }
        }
        
        controller.Move(moveSpeed * Time.fixedDeltaTime * moveDir);
    }

    void Jump()
    {
        jumpVector.y = jumpHeight;
        anim.SetTrigger("onJump");
        anim.SetBool("isOnAir", true);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        Vector3 inputDir = context.ReadValue<Vector2>();
        keyboardInputDirection = new Vector3(inputDir.x, 0, inputDir.y);
        //-------------------------
        if (keyboardInputDirection.sqrMagnitude > 0)
        {
            moveSpeed = walkSpeed * inputDir.normalized.sqrMagnitude;
        }
        else
        {
            moveSpeed = 0f;
        }
        // ------------------------

        if (controlMode == ControlMode.Normal)
        {
            keyboardInputDirection = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * keyboardInputDirection;
            if (keyboardInputDirection.sqrMagnitude > 0)
            {
                relativeMoveDir = Quaternion.LookRotation(keyboardInputDirection, Vector3.up);
            }
        }

        anim.SetFloat("Speed", moveSpeed);
    }

    protected virtual void OnAttack(InputAction.CallbackContext _)
    {
    }

    protected virtual void OnRightClick(InputAction.CallbackContext _)
    {
    }

    private void Heal_Performed(InputAction.CallbackContext _)
    {
        anim.SetTrigger("onHeal");
        //GameManager.Inst.Player_Stats.Heal();
    }

    protected virtual void SpecialAttack_Performed(InputAction.CallbackContext _)
    {
    }

    private void OnLockOn(InputAction.CallbackContext _)
    {
        TargetLock();
    }

    private void OnWeaponChange(InputAction.CallbackContext _)
    {
        weaponIndex = Weapons.Sword;
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame)
        {// Sword
            weaponIndex = Weapons.Sword;
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
        {// Arrow
            weaponIndex = Weapons.Bow;
        }
        playerWeapon.SwitchWeapon(weaponIndex);
        RefreshAnimator(weaponIndex);

        if(lockonTarget != null)
        {
            LockOff();
            TargetLock();
        }
    }

    private void RefreshAnimator(Weapons newWeapon)
    {
        anim = transform.GetChild((int)newWeapon).GetComponent<Animator>();
    }

    private void OnJumpInput(InputAction.CallbackContext _)
    {
        if (isGrounded)
        {
            Jump();
            jumpCounter++;
            return;
        }

        if (!isGrounded && jumpCounter > 0)
        {
            if (jumpCounter < 3 && actions.Player.Jump.triggered)
            {
                jumpCounter++;
                anim.SetTrigger("FlipJump");
                Jump();
            }
        }
    }

    public void TargetLock()
    {
        Physics.OverlapSphereNonAlloc(transform.position, lockOnRadius, lockonCollider, LayerMask.GetMask("Enemy"));

        if (lockonCollider.Length > 0)
        {
            if (lockonTarget == null)
            {
                float closestDistance = float.MaxValue;
                foreach (Collider coll in lockonCollider)
                {
                    if (coll == null)
                        break;
                    float distanceSqr = (coll.transform.position - transform.position).sqrMagnitude;
                    if (distanceSqr < closestDistance)
                    {// 거리가 더 짧을 경우 새로운 값 저장 
                        closestDistance = distanceSqr;
                        lockonTarget = coll.transform;
                    }
                }

                Transform lockOnEffectParent = lockonTarget.transform.Find("LockOnEffectPosition");
                lockOnEffect.transform.parent = lockOnEffectParent;
                lockOnEffect_Ground.transform.parent = lockonTarget.transform;

                lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);
                lockOnEffect_Ground.transform.position = lockonTarget.transform.position;

                lockOnEffect.SetActive(true);
                lockOnEffect_Ground.SetActive(true);
                controlMode = ControlMode.LockedOn;
            }
            else
            {
                LockOff();
            }
        }
        else
        {
            Debug.Log("No Lock on Target");
        }
    }

    void LockOff()
    {
        lockonTarget = null;

        lockOnEffect.transform.parent = null;
        lockOnEffect_Ground.transform.parent = null;

        lockOnEffect.SetActive(false);
        lockOnEffect_Ground.SetActive(false);

        controlMode = ControlMode.Normal;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, Vector3.up, lockOnRadius);

        Handles.color = Color.red;
        if (lockonTarget != null)
        {
            Handles.DrawLine(transform.position, lockonTarget.position, 3.0f);
        }
    }
#endif
}
