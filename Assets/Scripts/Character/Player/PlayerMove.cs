using System;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerMove : MonoBehaviour
{
    Player player;
    InputActions actions;
    Animator anim;
    CharacterController controller;

    PlayerState moveMode = PlayerState.Walk;
    public ControlMode controlMode = ControlMode.Normal;

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
    bool isGrounded = true;
    float jumpHeight = 5.0f;
    uint jumpCounter = 0;
    Vector3 jumpVector;
    Vector3 gravity;

    // ################ Target Lockon
    float lockOnRadius = 10f;
    [SerializeField] GameObject lockOnEffect;
    [SerializeField] GameObject lockOnEffect_Ground;
    private Transform lockTarget;

    // ################## Properties #####################
    public Vector3 LookDir => lookDir;

    private void Awake()
    {
        actions = new();
        anim = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();

        gravity = Physics.gravity;

        lockOnEffect.SetActive(false);
        lockOnEffect_Ground.SetActive(false);
    }

    private void Start()
    {
        player = GameManager.Inst.MainPlayer;
        moveSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        actions.Enable();
        actions.Player.Move.performed += OnMoveInput;
        actions.Player.Move.canceled += OnMoveInput;
        //actions.Player.Look.performed += OnLookInput;
        actions.Player.Jump.performed += OnJumpInput;
        actions.Player.Attack.performed += OnAttack;
        actions.Player.LockOn.performed += OnLockOn;
        actions.Player.WeaponChange.performed += OnWeaponChange;
    }

    private void OnDisable()
    {
        actions.Player.WeaponChange.performed -= OnWeaponChange;
        actions.Player.LockOn.performed -= OnLockOn;
        actions.Player.Jump.performed -= OnJumpInput;
        actions.Player.Attack.performed -= OnAttack;
        //actions.Player.Look.performed -= OnLookInput;
        actions.Player.Move.performed -= OnMoveInput;
        actions.Player.Move.canceled -= OnMoveInput;
        actions.Disable();
    }

    private void OnWeaponChange(InputAction.CallbackContext obj)
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {// Sword

        }
        else if(Keyboard.current.digit2Key.wasPressedThisFrame)
        {// Arrow

        }
    }

    private void OnLockOn(InputAction.CallbackContext _)
    {
        TargetLock();
    }

    private void Update()
    {
        Move_Turn();

        jumpVector += gravity * Time.deltaTime;
        controller.Move(jumpVector * Time.deltaTime);
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            anim.SetBool("onAir", false);
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
                    // 카메라를 기준으로 캐릭터가 자유롭게 움직이게
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed * Time.deltaTime);
                    break;
                case ControlMode.AimMode:
                    // 카메라가 바라보는 방향이 앞쪽이 되도록
                    Vector3 cameraForwardProjection = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
                    relativeMoveDir = Quaternion.LookRotation(cameraForwardProjection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed * Time.deltaTime);
                    moveDir = relativeMoveDir * moveDir;
                    break;
                case ControlMode.LockedOn:
                    // 카메라 위치를 기준으로 움직이는 방향이 결정됨
                    if (lockTarget != null)
                    {
                        relativeMoveDir = Quaternion.LookRotation(lockTarget.position - transform.position, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, relativeMoveDir, turnSpeed * Time.deltaTime);
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
        anim.SetBool("onAir", true);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        keyboardInputDirection = context.ReadValue<Vector3>();
        keyboardInputDirection.y = 0f;

        //------------------------- 임시
        if (keyboardInputDirection.sqrMagnitude > 0)
        {
            moveSpeed = walkSpeed;
        }
        else
        {
            moveSpeed = 0f;
        }
        // ------------------------

        if (controlMode == ControlMode.Normal)
        {
            keyboardInputDirection = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * keyboardInputDirection;
            relativeMoveDir = Quaternion.LookRotation(keyboardInputDirection, Vector3.up);
        }

        anim.SetFloat("Speed", moveSpeed);
        anim.SetFloat("MoveDir_X", moveDir.x);
        anim.SetFloat("MoveDir_Y", moveDir.y);
    }

    //CineMachine으로 대체
    //private void OnLookInput(InputAction.CallbackContext context)
    //{
    //    Vector3 inputDir = context.ReadValue<Vector2>();
    //    //inputDir.z = 1f;
    //    //lookDir = Camera.main.ScreenToWorldPoint(inputDir);
    //}

    private void OnAttack(InputAction.CallbackContext _)
    {
        anim.SetTrigger("onAttack");
        anim.SetFloat("ComboTimer", Mathf.Repeat(anim.GetCurrentAnimatorStateInfo(0).normalizedTime, 1.0f));
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
        Collider[] colls = Physics.OverlapSphere(transform.position, lockOnRadius, LayerMask.GetMask("Enemy"));
        //float closestDistance = float.MaxValue;
        //foreach (Collider coll in colls)
        //{
        //    float distanceSqr = (coll.transform.position - transform.position).sqrMagnitude;
        //    if (distanceSqr < closestDistance)
        //    {
        //        closestDistance = distanceSqr;
        //    }
        //}

        if (colls != null)
        {
            if (lockTarget == null)
            {
                Array.Sort(colls); // system 퀵소트
                lockTarget = colls[0].transform;

                Transform lockOnEffectParent = colls[0].transform.Find("LockOnEffectPosition");
                lockOnEffect.transform.parent = lockOnEffectParent;
                lockOnEffect_Ground.transform.parent = colls[0].transform;

                lockOnEffect.transform.position = lockOnEffectParent.position + new Vector3(0f, 0f, 0.6f);
                lockOnEffect_Ground.transform.position = colls[0].transform.position;

                lockOnEffect.SetActive(true);
                lockOnEffect_Ground.SetActive(true);
                controlMode = ControlMode.LockedOn;
                Debug.Log(lockTarget.transform.position);
            }
            else
            {
                LockOff();
                controlMode = ControlMode.Normal;
            }
        }
        else
        {
            Debug.Log("No Lock on Target");
        }
    }

    void LockOff()
    {
        lockTarget = null;

        lockOnEffect.transform.parent = null;
        lockOnEffect_Ground.transform.parent = null;

        lockOnEffect.SetActive(false);
        lockOnEffect_Ground.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.DrawWireDisc(transform.position, Vector3.up, lockOnRadius);

        Handles.color = Color.red;
        if (lockTarget != null)
        {
            Handles.DrawLine(transform.position, lockTarget.position, 3.0f);
        }
    }
#endif
}
