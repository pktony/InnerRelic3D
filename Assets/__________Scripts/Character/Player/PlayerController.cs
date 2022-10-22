using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 플레이어 컨트롤러 부모 클래스
/// </summary>
public abstract class PlayerController : MonoBehaviour
{
    protected GameManager gameManager;
    protected SoundManager soundManager;

    protected InputActions actions;
    protected Animator anim;
    protected CharacterController controller;
    protected AudioSource audioSource;
    private AudioSource footstepAudio;
    
    protected ControlMode prevControlMode;

    // ################### Move ######################3
    private Vector3 moveDir = Vector3.zero;
    private float moveSpeed = 0f;
    private Quaternion relativeMoveDir = Quaternion.identity;
    private bool isAttacking = false;

    [SerializeField] float walkSpeed = 3.0f;

    // ################### Look ########################
    private Vector3 keyboardInputDirection = Vector3.zero;
    private Vector2 lookDir = Vector2.zero;
    [SerializeField] float turnSpeed = 15f;

    // ################### Jump ########################
    private bool isGrounded = true;
    private int jumpCounter = 0;
    private float jumpHeight = 7f;
    private Vector3 jumpVector;
    private Vector3 gravity;
    private float fallMultiplier = 1f;

    // ################ Weapon Change
    private PlayerWeapons playerWeapon;
    
    // ################ 애니메이션 String 변수 캐싱 
    private readonly int OnJump = Animator.StringToHash("onJump");
    private readonly int IsOnAir = Animator.StringToHash("isOnAir");
    private readonly int FlipJump = Animator.StringToHash("FlipJump");
    private readonly int OnHeal = Animator.StringToHash("onHeal");
    private readonly int Speed = Animator.StringToHash("Speed");
    private readonly int IsDead = Animator.StringToHash("isDead");

    // ################## Properties #####################
    public Vector3 LookDir => lookDir;

    #region UNITY EVENT 함수 ###################################################
    protected virtual void Awake()
    {
        // 컴포넌트 --------------------------
        actions = new();
        controller = GetComponentInParent<CharacterController>();
        audioSource = transform.parent.GetComponent<AudioSource>();
        footstepAudio = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        anim.SetBool(IsDead, false);

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
    { // Input system 바인딩 활성화 / 등록 
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
    { // Input System 바인딩 비활성화 / 등록 해제 
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
        if (!gameManager.Player_Stats.IsDead)
        {
            Move_Turn();

            jumpVector += fallMultiplier * Time.deltaTime * gravity;
            controller.Move(jumpVector * Time.deltaTime);
            isGrounded = controller.isGrounded;

            if (isGrounded)
            {
                anim.SetBool("isOnAir", false);
                jumpCounter = 0;
                fallMultiplier = 1.0f;
            }
            else
            {
                if(controller.velocity.y < 0f)
                {
                    fallMultiplier = 4.0f;
                }
            }
        }
    }
    #endregion

    private void Move_Turn()
    {
        if (keyboardInputDirection.sqrMagnitude > 0f)
        {
            moveDir = keyboardInputDirection;

            Transform parent = transform.parent;
            switch (gameManager.Player_Stats.ControlMode)
            {
                case ControlMode.Normal:
                    // 플레이어가 카메라를 기준으로 방향을 결정한다 
                    parent.rotation = Quaternion.RotateTowards(parent.rotation,
                        relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.AimMode:
                    // 움직일 수 없고 회전만 가능하다 
                    moveDir = Vector3.zero;
                    parent.rotation = Quaternion.RotateTowards(parent.rotation,
                        relativeMoveDir, turnSpeed);
                    break;
                case ControlMode.LockedOn:
                    // 플레이어가 락온된 상대를 바라보며 카메라를 기준으로 움직인다 
                    if (gameManager.Player_Stats.LockonTarget != null)
                    {
                        relativeMoveDir = Quaternion.LookRotation(
                            gameManager.Player_Stats.LockonTarget.position - parent.position, Vector3.up);
                        parent.rotation = Quaternion.RotateTowards(
                            parent.rotation, relativeMoveDir, turnSpeed);
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
            moveSpeed = keyboardInputDirection.sqrMagnitude > 0 ?
                walkSpeed * inputDir.normalized.sqrMagnitude : 0f;
            // ------------------------

            if (gameManager.Player_Stats.ControlMode != ControlMode.LockedOn)
            {
                keyboardInputDirection = Quaternion.Euler(0, Camera.main!.transform.rotation.eulerAngles.y, 0) * keyboardInputDirection;
                if (keyboardInputDirection.sqrMagnitude > 0)
                {
                    relativeMoveDir = Quaternion.LookRotation(keyboardInputDirection, Vector3.up);
                }
            }

            anim.SetFloat(Speed, moveSpeed);
        }
    }

    protected virtual void OnAttack(InputAction.CallbackContext _) { }
    protected virtual void OnRightClick(InputAction.CallbackContext _) { }
    protected virtual void OnSpecialAttack(InputAction.CallbackContext _) { }

    private void Heal_Performed(InputAction.CallbackContext _)
    {
        if (!gameManager.Player_Stats.IsDead)
        {
            anim.SetTrigger(OnHeal);
            gameManager.Player_Stats.Heal();

            soundManager.PlaySound_Player(audioSource, PlayerClips.Heal);
        }
    }

    private void OnLockOn(InputAction.CallbackContext _)
    {
        gameManager.Player_Stats.TargetLock();
    }

    private void OnWeaponChange(InputAction.CallbackContext _)
    {// 무기 변경 함수 
        if (!gameManager.Player_Stats.IsDead)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame || Gamepad.current.leftShoulder.wasPressedThisFrame)
            {// Sword 로 변경 
                playerWeapon.SwitchWeapon(Weapons.Sword);
                gameManager.Player_Stats.SetWeapon(Weapons.Sword);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame)
            {// Archer 로 변경 
                playerWeapon.SwitchWeapon(Weapons.Bow);
                gameManager.Player_Stats.SetWeapon(Weapons.Bow);
            }
        }
    }

    private void OnJumpInput(InputAction.CallbackContext _)
    {
        if (isGrounded && !gameManager.Player_Stats.IsDead)
        {
            Jump(jumpCounter);
            jumpCounter++;
            soundManager.PlaySound_Player(audioSource, PlayerClips.Jump);
            return;
        }

        if (!isGrounded && jumpCounter < 2)
        {// 공중에 있고 점프 카운터가 1 이상일 때만 실행 
            Jump(jumpCounter);
            jumpCounter++;
            soundManager.PlaySound_Player(audioSource, PlayerClips.DoubleJump);
        }
    }

    private void Jump(int jumpCount)
    {
        jumpVector.y = jumpHeight;
        anim.SetBool(IsOnAir, true);
        if(jumpCount == 0)
        {
            anim.SetTrigger(OnJump);
        }
        else
        {
            anim.SetTrigger(FlipJump);
        }
    }

    public void PlayFootstepSound()
    {
        soundManager.PlaySound_Player(footstepAudio, PlayerClips.Footstep);
    }
}
