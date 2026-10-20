using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동
public class PlayerMove : MonoBehaviour
{
    private bool _isMoving = false;

    private const float Gravity = -20f;
    private const int MaxJumps = 2;
    private const float MaxStamina = 100f;

    private float _yVelocity = 0f; // 위 아래 속도, 매 프레임마다 중력 가속도가 붙는다.
    private int _jumpCount = 0; // 점프 횟수
    private float _stamina = 0f;

    [Header("스텟")]
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _jumpPower = 10f;
    [SerializeField] private float _staminaRecovery = 15f;
    [SerializeField] private float _sprintCost = 10f;
    [SerializeField] private float _jumpCost = 20f;

    [Header("컴포넌트")]
    [SerializeField] private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _stamina = MaxStamina;
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다
        // GetAxis = -1 ~ 1
        // GetAxisRaw = -1, 0, 1
        float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float v = Input.GetAxisRaw("Vertical"); // -1, 0, 1
        if (h != 0 || v != 0)
        {
            _isMoving = true;
        }
        else
        {
            _isMoving = false;
        }

        // 2. 키보드 입력에 따라 방향을 구한다.
        Vector3 direction = new Vector3(h, 0, v);
        direction.Normalize(); // 대각선이어도 방향의 길이를 1로 바꾸기 위해
        direction = transform.TransformDirection(direction);
        // 로컬 좌표계: 한 대상을 중심
        // 월드 좌표계: 세상을 중심

        // 3. 땅에 서 있으면 떨어지는 속도와 점프 횟수를 초기화한다.
        if (_characterController.isGrounded && _yVelocity < 0f)
        {
            _yVelocity = -1f;
            _jumpCount = 0;
        }

        // 4. 땅에 서있지 않을 때만 중력을 누적한다.
        if (!_characterController.isGrounded)
        {
            _yVelocity += Gravity * Time.deltaTime;
            if (_jumpCount == 0)
            {
                _jumpCount++;
            }
        }

        // 5. 만약에 점프키를 누르면 && 점프 카운트가 최대 횟수보다 낮으면 점프를 한다.
        if (Input.GetButtonDown("Jump") && _jumpCount < MaxJumps && _stamina >= _jumpCost)
        {
            _yVelocity = _jumpPower;
            _jumpCount++;
            _stamina -= _jumpCost;
        }


        // 6. 속도를 구한다.
        float requiredStamina = _sprintCost * Time.deltaTime;
        bool isSprint = Input.GetButton("Sprint") && _stamina >= 0;
        float currentSpeed = _moveSpeed;
        if (isSprint && _isMoving)
        {
            if (_stamina >= _sprintCost)
            {
                _stamina -= requiredStamina;
                currentSpeed = _sprintSpeed;
            }
        }
        else
        {
            _stamina = Mathf.Min(MaxStamina, _stamina + _staminaRecovery * Time.deltaTime);
        }

        Vector3 velocity = direction * currentSpeed;
        velocity.y = _yVelocity;

        // 7. 속도(방향과 속력)에 따라 이동한다.
        // transform.position += _moveSpeed * Time.deltaTime * direction;
        _characterController.Move(Time.deltaTime * velocity);
    }
}
