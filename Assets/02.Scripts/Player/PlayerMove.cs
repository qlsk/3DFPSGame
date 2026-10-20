using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동
public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private float _yVelocity = 0f; // 위 아래 속도, 매 프레임마다 중력 가속도가 붙는다.
    private readonly float _maxStamina = 100f;
    private float _currentStamina = 100f;
    private readonly float _staminaRegenRate = 1f;
    private readonly float _runSpeed = 12f;

    private float _jumpReduceStamina = 10f;
    private float _runReduceStamina = 1f;
    private bool _isRunning = false;
    private readonly int _maxJumpCount = 2;
    private int _currentJumpCount = 0;
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private CharacterController _characterController;

    [SerializeField] private PlayerStaminaSlider _playerStaminaSlider;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 3. 땅에 서 있으면 떨어지는 속도를 초기화한다.
        if (_characterController.isGrounded && _yVelocity < 0f)
        {
            _yVelocity = -2f;
            _currentJumpCount = 0;
        }

        // 4. 땅에 서있지 않을 때만 중력을 누적한다.
        if (!_characterController.isGrounded)
        {
            _yVelocity += Gravity * Time.deltaTime;
            // 점프를 하지 않고 떨어질 때 점프 횟수 추가
            if (_currentJumpCount == 0)
            {
                _currentJumpCount = 1;
            }
        }

        // 5. 만약에 점프키를 누르면 점프를 한다.
        if (Input.GetButtonDown("Jump") && CanJump()) // 중재자 패턴
        {
            GameLogger.Log("jump");
            _yVelocity = _jumpPower;
            _currentJumpCount++;
            ReduceStamina(_jumpReduceStamina);
        }

        Move();
        RegenStamina();
    }

    private void Move()
    {
        // 1. 키보드 입력을 받는다
        // GetAxis = -1 ~ 1
        // GetAxisRaw = -1, 0, 1
        float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float v = Input.GetAxisRaw("Vertical"); // -1, 0, 1

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0, v);
        direction.Normalize(); // 대각선이어도 방향의 길이를 1로 바꾸기 위해
        direction = transform.TransformDirection(direction);
        // 로컬 좌표계: 한 대상을 중심
        // 월드 좌표계: 세상을 중심

        // LeftShitft Key가 눌리면 Run
        float moveSpeed = _moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift) && IsRemainStamina())
        {
            moveSpeed = _runSpeed;
            // 스테미나 소모
            ReduceStamina(_runReduceStamina * Time.deltaTime);
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }

        // 6. 속도를 구한다.
        Vector3 velocity = direction * moveSpeed;
        velocity.y += _yVelocity;

        // 7. 속도(방향과 속력)에 따라 이동한다.
        // transform.position += _moveSpeed * Time.deltaTime * direction;
        _characterController.Move(Time.deltaTime * velocity);
    }

    private void RegenStamina()
    {
        if (_currentStamina < _maxStamina && CanRegenStamina())
        {
            _currentStamina += _staminaRegenRate * Time.deltaTime;
            GameLogger.Log("regen");
            _playerStaminaSlider.UpdateStaminaSlider(_currentStamina);
        }
    }

    private bool CanRegenStamina()
    {
        // 달리는 중이거나 점프 중이면 스테미나 회복 불가
        return !_isRunning && _characterController.isGrounded;
    }

    private void ReduceStamina(float value)
    {
        if (value < 0f && _currentStamina <= 0f) return;
        _currentStamina -= value;
        _playerStaminaSlider.UpdateStaminaSlider(_currentStamina);
    }

    private bool IsRemainStamina()
    {
        return _currentStamina > 0f;
    }

    private bool CanJump()
    {
        return _currentJumpCount < _maxJumpCount && IsRemainStamina();
    }
}
