using UnityEngine;


public class MoveSystem : IMove
{
    private  CharacterController _controller;
    private  Animator _animator;
    private  IInput _playerInput;
    private  Transform _transform;



    private float _walkSpeed_F, _walkSpeed_B, _walkSpeed_LR, _runSpeed,_rotationSmoothTime, _speedSmmothTime;
    private float _gravity, _jumpHeight;
    private float _currentVelocityY, _targetRotation, _currentSpeed, _speedVelocity, _rotationVelocity;


    private Vector3 _moveDirection;
    private Vector2 _input;

    private AnimationManager _animationManager;

    private Camera _camera;
    private int AimHash;
    private int JumpHash;

    private float _minX, _maxX, _minZ, _maxZ;
    private float _wrapPadding = 0.1f;

    private float _speedMultiplier = 1f;
    private float _jumpMultiplier = 1f;

    public void ApplyMultiplierSpeed(float value) => _speedMultiplier *= value;
    public void RemoveMultiplierSpeed(float value) => _speedMultiplier /= value;
    public void ApplyJumpMultiplier(float value) => _jumpMultiplier *= value;
    public void Initialize(CharacterController controller, Animator animator, IInput playerInput, Transform transform, MoveData config, float minX, float maxX, float minZ, float maxZ)
    {
        _controller = controller;
        _animator = animator;
        _playerInput = playerInput;
        _transform = transform;

        _walkSpeed_F = config.walkF;
        _walkSpeed_B = config.walkB;
        _walkSpeed_LR = config.walkLR;

        _runSpeed = config.run;
        _rotationSmoothTime = config.rotationSmooth;
        _speedSmmothTime = config.speedSmooth;
        
        _gravity = config.gravity;
        _jumpHeight = config.jumpHeight;

        _animationManager = GameManager.Instance.AnimationManager;
        AimHash = AnimationManager.AIM;
        JumpHash = AnimationManager.JUMP;

        _camera = Camera.main;

        _minX = minX;
        _maxX = maxX;
        _minZ = minZ;
        _maxZ = maxZ;

    }


    public void Tick()
    {
        Gravity();
        Move();
        _animationManager.SetBool(_animator, AimHash, _playerInput.IsAim);
    }

    public void Move()
    {
        GetInput();
        CalculateSpeed(); // 속도 계산
        CalculateDirection(); // 이동 방향 계산
        ApplyRotation(); // 회전 적용
        ApplyMovement(); // 이동 적용

        CheckPosition();
        _animationManager.UpdateAnimationForPlayer(_animator, _currentSpeed, _controller.isGrounded, _currentVelocityY, _input); // 애니메이션 적용
    }

    private void GetInput() => _input = _playerInput.MoveInput;
    private void CalculateSpeed()
    {
        float target = 0f;
        if (_playerInput.IsRun) target = _runSpeed;
        else
        {
            if (_input.y > 0.1f) target = _walkSpeed_F;
            else if (_input.y < -0.1f) target = _walkSpeed_B;
            else if (Mathf.Abs(_input.x) > 0.1f) target = _walkSpeed_LR;
        }
        target *= _input.magnitude;

        target *= _speedMultiplier; // 속도 증가

        _currentSpeed = Mathf.SmoothDamp(_currentSpeed, target, ref _speedVelocity, _speedSmmothTime);
    }
    private void CalculateDirection()
    {
        float cameraY = _camera.transform.eulerAngles.y;
        _moveDirection = Quaternion.Euler(0f, cameraY, 0f) * new Vector3(_input.x, 0, _input.y);
    }

    private void ApplyRotation()
    {
        bool isRotation = false;
        bool IsAIm = _playerInput.IsAim;
        if (IsAIm)
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 200f,  ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 target = hit.point - _transform.position;
                target.y = 0f;

                _targetRotation = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg;

                isRotation = true;
            }
        }else if(_input != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(_moveDirection.x, _moveDirection.z) * Mathf.Rad2Deg;
            isRotation = true;
        }

        if (isRotation)
        {
            float rotationSmmoth = IsAIm ? 0.0f : _rotationSmoothTime;
            float rotation = Mathf.SmoothDampAngle(_transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmmoth);

            _transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

    }
    private void ApplyMovement()
    {
        Vector3 velocity = _moveDirection.normalized * _currentSpeed;
        velocity.y = _currentVelocityY;

        _controller.Move(velocity * Time.deltaTime);
    }

    private void Gravity()
    {
        bool ground = _controller.isGrounded;

        if (ground)
        {
            if (_currentVelocityY < 0.0f) _currentVelocityY = -2.0f;
            if (_playerInput.IsJump)
            {
                float jumpH = Mathf.Max(0.01f, _jumpHeight * _jumpMultiplier);
                _currentVelocityY = Mathf.Sqrt(jumpH * -2f * _gravity);// Sqart( h * -2 * g )
                _animationManager.SetTrigger(_animator, JumpHash);
            }
        }
        else _currentVelocityY += _gravity * Time.deltaTime;

    }

    private void CheckPosition()
    {
        Vector3 pos = _transform.position;
        bool wrapped = false;

        if(pos.x > _maxX) { pos.x = _minX + _wrapPadding; wrapped = true; }
        else if(pos.x < _minX) { pos.x = _maxX - _wrapPadding; wrapped = true; }

        if (pos.z > _maxZ) { pos.z = _minZ + _wrapPadding; wrapped = true; }
        else if (pos.z < _minZ) { pos.z = _maxZ - _wrapPadding; wrapped = true; }

        if (!wrapped) return;

        bool check = _controller.enabled;
        if (check) _controller.enabled = false; // 만약을 위해서, 순간 이동시에 토글해서 안전하게
        _transform.position = pos;
        if (check) _controller.enabled = true;
    }


}
