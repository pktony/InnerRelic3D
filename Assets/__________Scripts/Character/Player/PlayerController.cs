using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PlayerController : MonoBehaviour
{
    protected InputActions actions;
    protected Animator anim;
    protected CharacterController controller;

    public GameObject mainCam;
    public CinemachineVirtualCamera lockonCam;

    PlayerState moveMode = PlayerState.Walk;
    protected ControlMode controlMode = ControlMode.Normal;
    protected ControlMode prevControlMode;

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
    private Collider[] lockonCollider = new Collider[5];
    private Transform lockonFollowTarget;

    // ################ Weapon Change
    PlayerWeapons playerWeapon;

    // ################## Properties #####################
    public Vector3 LookDir => lookDir;

    public ControlMode ControlMode
    {
        get => controlMode;
        set
        {
            controlMode = value;
        }
    }

    protected virtual void Awake()
    {
        actions = new();
        controller = GetComponentInParent<CharacterController>();
        anim = GetComponent<Animator>();
        lockonFollowTarget = transform.parent.GetChild(2);

        playerWeapon = GetComponentInParent<PlayerWeapons>();

        gravity = Physics.gravity;

        lockOnEffect.SetActive(false);
    }

    private void Start()
    {
        moveSpeed = walkSpeed;
        //Cursor.lockState = CursorLockMode.Locked;
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
        actions.Player.SpecialAttack.performed += OnSpecialAttack;
        actions.Player.SpecialAttack.canceled += OnSpecialAttack;
        actions.Player.Heal.performed += Heal_Performed;
        actions.Player.LockOn.performed += OnLockOn;
        actions.Player.WeaponChange.performed += OnWeaponChange;
    }

    private void OnDisable()
    {
        actions.Player.SpecialAttack.performed -= OnSpecialAttack;
        actions.Player.SpecialAttack.canceled -= OnSpecialAttack;
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
                    transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation, relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.AimMode:
                    // 움직일 수 없고 회전만 가능하다 
                    moveDir = Vector3.zero;
                    transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation,
                        Quaternion.LookRotation( keyboardInputDirection), turnSpeed);
                    break;
                case ControlMode.LockedOn:
                    // 플레이어가 락온된 상대를 바라보며 카메라를 기준으로 움직인
                    if (GameManager.Inst.Player_Stats.LockonTarget != null)
                    {
                        relativeMoveDir = Quaternion.LookRotation(GameManager.Inst.Player_Stats.LockonTarget.position - transform.parent.position, Vector3.up);
                        transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation, relativeMoveDir, turnSpeed);
                        moveDir = relativeMoveDir * moveDir;

                        //Vector3 dir = mainCam.transform.position - transform.position;
                        //dir = dir.normalized;
                        //dir.y = 0f;

                        //lockOnCamera.m_XAxis.Value = Vector3.Angle(dir, mainCam.transform.forward);
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

    protected virtual void OnAttack(InputAction.CallbackContext _) { }
    protected virtual void OnRightClick(InputAction.CallbackContext _) { }
    protected virtual void OnSpecialAttack(InputAction.CallbackContext _) { }

    private void Heal_Performed(InputAction.CallbackContext _)
    {
        anim.SetTrigger("onHeal");
        GameManager.Inst.Player_Stats.Heal();
    }


    private void OnLockOn(InputAction.CallbackContext _)
    {
        TargetLock();
    }

    private void OnWeaponChange(InputAction.CallbackContext _)
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame)
        {// Sword 로 변경 
            playerWeapon.SwitchWeapon(Weapons.Sword);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
        {// Archer 로 변경 
            playerWeapon.SwitchWeapon(Weapons.Bow);
        }
        
        if (GameManager.Inst.Player_Stats.LockonTarget != null)
        {
            LockOff();
            TargetLock();
        }
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

    private void TargetLock()
    {
        Physics.OverlapSphereNonAlloc(transform.position, lockOnRadius, lockonCollider, LayerMask.GetMask("Enemy"));

        if (lockonCollider.Length > 0)
        {
            if (GameManager.Inst.Player_Stats.LockonTarget == null)
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
                        GameManager.Inst.Player_Stats.LockonTarget = coll.transform;
                    }
                }
                Transform target = GameManager.Inst.Player_Stats.LockonTarget;
                target.GetComponent<Enemy>().onDie = LockOff;
                transform.parent.rotation = Quaternion.LookRotation(target.position - transform.parent.position);

               //Test.position = target.position;

                Transform lockOnEffectParent = target.transform.Find("LockOnEffectPosition");
                lockOnEffect.transform.parent = lockOnEffectParent;
                lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);

                lockOnEffect.SetActive(true);

                mainCam.SetActive(false);
                lockonFollowTarget.gameObject.SetActive(true);
                lockonCam.LookAt = target;
                lockonCam.gameObject.SetActive(true);

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

    private void LockOff()
    {
        GameManager.Inst.Player_Stats.LockonTarget = null;
        lockOnEffect.transform.parent = null;

        lockOnEffect.SetActive(false);

        lockonFollowTarget.gameObject.SetActive(false);
        lockonCam.LookAt = null;
        lockonCam.gameObject.SetActive(false);
        mainCam.transform.position = lockonCam.transform.position;
        mainCam.SetActive(true);

        controlMode = ControlMode.Normal;
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmos()
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(transform.position, Vector3.up, lockOnRadius, 3.0f);

    }
#endif
}
