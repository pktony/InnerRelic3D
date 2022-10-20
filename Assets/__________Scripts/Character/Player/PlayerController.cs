using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PlayerController : MonoBehaviour
{
    protected GameManager gameManager;
    protected SoundManager soundManager;

    protected InputActions actions;
    protected Animator anim;
    protected CharacterController controller;
    protected AudioSource audioSource;
    
    protected ControlMode prevControlMode;

    // ################### Move ######################3
    private Vector3 moveDir = Vector3.zero;
    private float moveSpeed = 0f;
    private Quaternion relativeMoveDir = Quaternion.identity;
    private bool isAttacking = false;

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

    // ################ Weapon Change
    PlayerWeapons playerWeapon;

    // ################## Properties #####################
    public Vector3 LookDir => lookDir;


    protected virtual void Awake()
    {
        // 컴포넌트 --------------------------
        actions = new();
        controller = GetComponentInParent<CharacterController>();
        audioSource = GetComponentInParent<AudioSource>();
        anim = GetComponent<Animator>();
        anim.SetBool("isDead", false);

        // 기타 --------------------------
        gravity = Physics.gravity;
        playerWeapon = GetComponentInParent<PlayerWeapons>();
        moveSpeed = walkSpeed;
    }

    protected virtual void Start()
    {
        gameManager = GameManager.Inst;
        soundManager = SoundManager.Inst;
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
        if (!gameManager.Player_Stats.IsDead && !isAttacking)
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
    }

    private void Move_Turn()
    {
        if (keyboardInputDirection.sqrMagnitude > 0f)
        {
            moveDir = keyboardInputDirection;

            switch (gameManager.Player_Stats.ControlMode)
            {
                case ControlMode.Normal:
                    // 플레이어가 카메라를 기준으로 방향을 결정한다 
                    transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation,
                        relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.AimMode:
                    // 움직일 수 없고 회전만 가능하다 
                    moveDir = Vector3.zero;
                    transform.parent.rotation = Quaternion.RotateTowards(transform.parent.rotation,
                        relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.LockedOn:
                    // 플레이어가 락온된 상대를 바라보며 카메라를 기준으로 움직인
                    if (gameManager.Player_Stats.LockonTarget != null)
                    {
                        relativeMoveDir = Quaternion.LookRotation(
                            gameManager.Player_Stats.LockonTarget.position - transform.parent.position, Vector3.up);
                        transform.parent.rotation = Quaternion.RotateTowards(
                            transform.parent.rotation, relativeMoveDir, turnSpeed);
                        moveDir = relativeMoveDir * moveDir;
                    }
                    break;
            }
        }
        controller.Move(moveSpeed * Time.fixedDeltaTime * moveDir);
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        if (!gameManager.Player_Stats.IsDead)
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

            if (gameManager.Player_Stats.ControlMode != ControlMode.LockedOn)
            {
                keyboardInputDirection = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0) * keyboardInputDirection;
                if (keyboardInputDirection.sqrMagnitude > 0)
                {
                    relativeMoveDir = Quaternion.LookRotation(keyboardInputDirection, Vector3.up);
                }
            }

            anim.SetFloat("Speed", moveSpeed);
        }
    }

    protected virtual void OnAttack(InputAction.CallbackContext _) { isAttacking = true; }
    protected virtual void OnRightClick(InputAction.CallbackContext _) { }
    protected virtual void OnSpecialAttack(InputAction.CallbackContext _) { }

    private void Heal_Performed(InputAction.CallbackContext _)
    {
        if (!gameManager.Player_Stats.IsDead)
        {
            anim.SetTrigger("onHeal");
            gameManager.Player_Stats.Heal();

            soundManager.PlaySound_Player(audioSource, PlayerClips.Heal);
        }
    }

    private void OnLockOn(InputAction.CallbackContext _)
    {
        gameManager.Player_Stats.TargetLock();
    }

    private void OnWeaponChange(InputAction.CallbackContext _)
    {
        if (!gameManager.Player_Stats.IsDead)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame)
            {// Sword 로 변경 
                playerWeapon.SwitchWeapon(Weapons.Sword);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
            {// Archer 로 변경 
                playerWeapon.SwitchWeapon(Weapons.Bow);
            }

            if (gameManager.Player_Stats.LockonTarget != null)
            {
                gameManager.Player_Stats.LockOff();
                gameManager.Player_Stats.TargetLock();
            }
        }
    }

    private void OnJumpInput(InputAction.CallbackContext _)
    {
        if (isGrounded && !gameManager.Player_Stats.IsDead)
        {
            Jump();
            soundManager.PlaySound_Player(audioSource, PlayerClips.Jump);
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
                soundManager.PlaySound_Player(audioSource, PlayerClips.DoubleJump);
            }
        }
    }

    void Jump()
    {
        jumpVector.y = jumpHeight;
        anim.SetTrigger("onJump");
        anim.SetBool("isOnAir", true);
    }

    public void SetNotAttacking()
    {// 애니메이션 이벤트 함수 
        isAttacking = false;
    }
}
