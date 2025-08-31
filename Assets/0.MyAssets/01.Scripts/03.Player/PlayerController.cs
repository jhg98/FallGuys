using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviourPun
{
    [Header("Player")]
    public bool CanControlPlayer = false;
    public bool CanControlCamera = false;

    [Space(10)]
    public float moveSpeed = 5f;
    public float acceleration = 10f;
    //public float accelerationInAir = 1f;
    public float rotationSpeed = 15f;
    //public float rotationSpeedInAir = 15f;
    public float animationChangeRate = 10.0f;

    [Space(10)]
    public float jumpHeight = 1.5f;
    public Transform groundChecker;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Space(10)]
    public float knockdownForceThreshold = 10f;
    public float recoverySpeed = 2f;

    [Space(10)]
    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Header("Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 30.0f;
    public float mouseSensitivity = 2.0f;

    // input
    private float mouseXInput = 0;
    private float mouseYInput = 0;
    private float xInput = 0;
    private float yInput = 0;
    private bool jumpInput = false;
    private float targetRotation = 0;

    // player
    private Rigidbody rb;
    private bool Grounded;
    private bool isKnockdown;

    // cinemachine
    private GameObject _mainCamera;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // animation
    private Animator _animator;
    private float _animationBlend;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |  RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezeRotationY;

        // ThirdPerson 에셋의 메인카메라 사용
        _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }
    private void Start()
    {
        if (photonView != null && photonView.IsMine)
        {
            GameManager.Instance.RegisterPlayer(this);
        }

        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _animator = GetComponent<Animator>();

        AssignAnimationIDs();
    }
    void Update()
    {
        if (photonView != null && !photonView.IsMine)
            return;

        // 입력은 Update에서
        HandleInput();
    }
    void FixedUpdate()
    {
        if (photonView != null && !photonView.IsMine)
            return;

        Move();
        JumpAndFall();
        GroundedCheck();

        // 넘어진 상태에서 조작 가능하다면 일어서기 시도
        if (isKnockdown && CanControlPlayer)
            TryStandUp();
    }
    private void LateUpdate()
    {
        if (photonView != null && !photonView.IsMine)
            return;

        CameraRotation();
    }
    private void OnApplicationFocus(bool focus)
    {
        if (photonView != null && !photonView.IsMine)
            return;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // 외부로부터 힘을 받는 메서드
    public void Hit(Vector3 force, float stunTime)
    {
        rb.AddForce(force, ForceMode.Impulse);

        float impactForce = force.magnitude;
        if (impactForce < knockdownForceThreshold)
        {
            // 약한 힘이면 밀리기만 함
            StartCoroutine(EnterKnockback(stunTime));
        }
        else
        {
            // 강한 힘이면 넘어짐
            StartCoroutine(EnterKnockdown(stunTime));
        }
    }
    private IEnumerator EnterKnockback(float stunTime)
    {
        CanControlPlayer = false;

        yield return new WaitForSeconds(stunTime);

        CanControlPlayer = true;
    }
    private IEnumerator EnterKnockdown(float stunTime)
    {
        CanControlPlayer = false;

        isKnockdown = true;
        rb.constraints = RigidbodyConstraints.None;

        yield return new WaitForSeconds(stunTime);

        CanControlPlayer = true;
    }
    private void TryStandUp()
    {
        // 일어서는 동안엔 다시 넘어지면 안됨
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 현재 회전값
        Quaternion current = transform.rotation;
        // 목표 회전값
        Quaternion target = Quaternion.Euler(0f, current.eulerAngles.y, 0f);

        // 천천히 회전
        transform.rotation = Quaternion.Slerp(current, target, Time.fixedDeltaTime * recoverySpeed);

        // 오차가 작으면 일어섰다고 판단
        if (Quaternion.Angle(current, target) < 10f)
        {
            transform.rotation = target; // 보정
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            isKnockdown = false;
        }
    }

    private void HandleInput()
    {
        if (CanControlPlayer)
        {
            if (isKnockdown) return;

            xInput = Input.GetAxis("Horizontal");
            yInput = Input.GetAxis("Vertical");

            if (Grounded && Input.GetKeyDown(KeyCode.Space))
                jumpInput = true;
        }

        if (CanControlCamera)
        {
            mouseXInput = Input.GetAxis("Mouse X") * mouseSensitivity;
            mouseYInput = Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
    }
    private void CameraRotation()
    {
        if (!CanControlCamera) return;

        _cinemachineTargetYaw += mouseXInput;
        _cinemachineTargetPitch -= mouseYInput;
        _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0f
        );
    }
    private void Move()
    {
        if (!CanControlPlayer) return;

        if (isKnockdown) return;

        // 입력 방향
        Vector3 inputDirection = new Vector3(xInput, 0f, yInput).normalized;

        // 입력 끝나면 자동으로 카메라 방향으로 회전하는 문제
        if (inputDirection != Vector3.zero)
        {
            // 부드럽게 회전
            //targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
            //    _mainCamera.transform.eulerAngles.y;
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                _cinemachineTargetYaw; // 카메라 튀는 문제
            Quaternion rotation = Quaternion.Euler(0f, targetRotation, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
        }

        // 카메라가 바라보는 방향을 앞으로 설정
        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        // 가속/감속 이동. 입력이 없을 땐 속도 0으로 설정.
        Vector3 targetVelocity = inputDirection == Vector3.zero ? Vector3.zero : targetDirection * moveSpeed;
        Vector3 velocityChange = (targetVelocity - new Vector3(rb.velocity.x, 0f, rb.velocity.z));
        rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);

        // 애니메이션 적용
        float targetSpeed = targetVelocity.magnitude;
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * animationChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, 1f); // 바로 적용
    }
    private void JumpAndFall()
    {
        if (!CanControlPlayer) return;

        //if (isKnockdown) return; // 일어서는 중에 떨어질 수도 있음

        if (Grounded)
        {
            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);

            if (jumpInput)
            {
                float gravity = Physics.gravity.magnitude;
                float jumpForce = Mathf.Sqrt(2 * gravity * jumpHeight);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                _animator.SetBool(_animIDJump, true);

                jumpInput = false;
            }
        }
        else
        {
            _animator.SetBool(_animIDFreeFall, true);
        }
    }
    private void GroundedCheck()
    {
        Grounded = Physics.CheckSphere(groundChecker.position, groundCheckRadius, groundLayer);

        // 넘어졌을 땐 바닥을 딛지 않은 걸로 판단
        if(isKnockdown) Grounded = false;

        _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(transform.position), FootstepAudioVolume);
            }
        }
    }
    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(transform.position), FootstepAudioVolume);
        }
    }

    private void OnDrawGizmos()
    {
        if (Grounded) Gizmos.color = Color.green;
        else Gizmos.color = Color.red;

        Gizmos.DrawSphere(groundChecker.position, groundCheckRadius);
    }
}
