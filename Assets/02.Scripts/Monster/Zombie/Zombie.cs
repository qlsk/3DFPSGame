using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// FSM(유한 상태 머신): 유한개의 상태가 있고, 상태마다 행동을 다르게 하여 AI를 구현하는 유명한 방법
// - "상태": 객체가 가질 수 있는 어떤 조건이나 상황
// - "전이": 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - "머신": 위 방식을 따라 규칙을 세워둔 것

// 좀비가 가질 수 있는 상태
public enum ZombieState
{
    Idle, // 대기: 제자리에서 플레이어를 기다린다.
    Patrol, // 순찰: 두 지점을 오가며 순찰한다.
    Roam, // 배회: 처음 자리 주변의 랜덤한 지점으로 돌아다닌다.
    Trace, // 추적: 플레이어를 쫓아간다.
    Comeback, // 복귀: 처음 자리로 돌아간다.
    Attack, // 공격: 일정 시간마다 플레이어를 때린다.
    Hit, // 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
    Death // 죽음: 스러지고 잠시 뒤 사라진다.
}

public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스텟")]
    [SerializeField] private float _detectDistance = 8f; // 플레이어를 발견하는 거리
    [SerializeField] private float _comebackDistance = 12f; // 플레이어 추적을 포기하는 거리
    [SerializeField] private float _attackRange = 2f; // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f; // 공격 간격
    [SerializeField] private float _moveSpeed = 5f; // 이동속도
    [SerializeField] private float _maxHealth = 30f; // 최대 체력
    [SerializeField] private float _attackDamage = 10f; // 대미지
    public float MaxHealth => _maxHealth;
    [SerializeField] private Vector3 _moveDestination = new Vector3(10, 0, 0);

    [Header("배회")]
    [SerializeField] private float _roamRadius = 8f; // 처음 자리 기준 배회 반경 (복귀 거리보다 작아야 한다)
    [SerializeField] private float _roamSpeed = 2f; // 배회 이동속도
    [SerializeField] private Vector2 _roamWaitRange = new Vector2(1f, 3f); // 도착 후 쉬는 시간 (최소, 최대)
    [SerializeField] private float _roamTimeout = 6f; // 목적지에 못 가는 경우 포기하는 시간

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodEffectPrefab;

    private float _attackTimer = 0f;
    private CharacterController _controller;
    private Vector3 _startPosition;
    private Vector3 _endPosition;
    private GameObject _player;
    private float _currentHealth;
    public float CurrentHealth => _currentHealth;
    private ZombieState _state = ZombieState.Idle;
    private Vector3 _targetPosition;
    private Vector3 _roamTarget;
    private float _roamTimer;
    private float _waitTimer;

    public event Action OnDamaged;
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _controller = GetComponent<CharacterController>();
        _startPosition = transform.position;
        _endPosition = _startPosition + _moveDestination;
        _targetPosition = _endPosition;
        StartRoam();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;
            case ZombieState.Patrol:
                Patrol();
                break;
            case ZombieState.Roam:
                Roam();
                break;
            case ZombieState.Trace:
                Trace();
                break;
            case ZombieState.Comeback:
                Comeback();
                break;
            case ZombieState.Attack:
                Attack();
                break;
            case ZombieState.Hit:
                break;
            case ZombieState.Death:
                break;
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, _startPosition) < 1f)
        {
            _targetPosition = _endPosition;
        }
        else if (Vector3.Distance(transform.position, _endPosition) < 1f)
        {
            _targetPosition = _startPosition;
        }

        Vector3 direction = _targetPosition - transform.position;
        direction.y = 0;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(_moveSpeed * direction);

        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _detectDistance)
        {
            GameLogger.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
        }
    }

    // 배회: 처음 자리 주변의 랜덤한 지점으로 걸어간다.
    private void Roam()
    {
        _roamTimer += Time.deltaTime;

        Vector3 direction = _roamTarget - transform.position;
        direction.y = 0;

        // 도착했거나 너무 오래 걸리면(벽에 막힘 등) 잠깐 쉰다.
        if (direction.magnitude < 0.5f || _roamTimer >= _roamTimeout)
        {
            _waitTimer = UnityEngine.Random.Range(_roamWaitRange.x, _roamWaitRange.y);
            GameLogger.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;
            return;
        }

        direction.Normalize();
        transform.forward = direction;
        _controller.SimpleMove(_roamSpeed * direction);

        CheckDetectPlayer();
    }

    private void StartRoam()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * _roamRadius;
        _roamTarget = _startPosition + new Vector3(offset.x, 0f, offset.y);
        _roamTimer = 0f;
        _state = ZombieState.Roam;
    }

    // 플레이어가 감지 거리 안에 있으면 Trace 상태로 전이한다.
    private void CheckDetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _detectDistance)
        {
            GameLogger.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
        }
    }

    // 대미지를 받는다.
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death) return;
        _currentHealth = Mathf.Clamp(_currentHealth - damageInfo.Value, 0f, _maxHealth);
        StopAllCoroutines();
        if (_currentHealth > 0f)
        {
            // 좀비의 자식으로 블러드 스프레이를 생성 -> 위치 수정
            GameObject bloodSpray = Instantiate(_bloodEffectPrefab, gameObject.transform);
            // bloodSpray.transform.SetParent(transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;
            // 잠깐 멈추고 싶다
            GameLogger.Log($"{_state} -> Hit");
            _state = ZombieState.Hit;
            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            GameLogger.Log($"{_state} -> Death");
            _state = ZombieState.Death;
            StartCoroutine(Death_Coroutine());
            OnDeath?.Invoke();
        }

        OnDamaged?.Invoke();
    }

    private IEnumerator Hit_Coroutine()
    {
        yield return new WaitForSeconds(0.3f);
        GameLogger.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private IEnumerator Death_Coroutine()
    {
        _controller.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void Idle()
    {
        // TODO: Idle 애니메이션 실행
        // 대기: 제자리에서 플레이어를 기다린다.
        // 플레이어와 가까워지면 Trace 상태로 전이한다.
        CheckDetectPlayer();
        if (_state != ZombieState.Idle) return;

        // 쉬는 시간이 끝나면 다시 배회한다.
        _waitTimer -= Time.deltaTime;
        if (_waitTimer <= 0f)
        {
            GameLogger.Log($"{_state} -> Roam");
            StartRoam();
        }
    }

    // 추적: 플레이어를 쫓아간다.
    private void Trace()
    {
        // TODO: Run 애니메이션 실행

        // 처음 자리에서 너무 멀어지면 돌아간다.
        // if (Vector3.Distance(transform.position, _startPosition) > _comebackDistance)
        // {
        //     GameLogger.Log($"{_state} -> Idle");
        //     _state = ZombieState.Idle;
        //     return;
        // }

        // 플레이어가 공격 거리 안이면 공격한다.
        if (Vector3.Distance(transform.position, _player.transform.position) <= _attackRange)
        {
            _attackTimer = 0f;
            GameLogger.Log($"{_state} -> Attack");
            _state = ZombieState.Attack;
            return;
        }

        if (Vector3.Distance(transform.position, _player.transform.position) >= _comebackDistance)
        {
            GameLogger.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;
            return;
        }

        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        transform.forward = direction;
        _controller.SimpleMove(_moveSpeed * direction);
    }

    private void Comeback()
    {
        // 처음 자리에 도달하면 다시 배회한다.
        if (Vector3.Distance(transform.position, _startPosition) < 1f)
        {
            GameLogger.Log($"{_state} -> Roam");
            StartRoam();
            return;
        }

        // 처음 자리로 되돌아감
        Vector3 direction = _startPosition - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.forward = direction;
        _controller.SimpleMove(_moveSpeed * direction);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        // 플레이어가 공격 사정 거리 밖으로 나가면 Trace
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > _attackRange)
        {
            GameLogger.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackInterval)
        {
            // TODO : 공격 애니메이션 실행
            DamageInfo damageInfo = new DamageInfo()
            {
                Value = _attackDamage,
                HitPoint = _player.transform.position,
                HitNormal = (transform.position - _player.transform.position).normalized,
            };
            _player.GetComponent<IDamageable>().TakeDamage(damageInfo);
            GameLogger.Log("플레이어를 때렸습니다.");
            _attackTimer = 0;
        }
    }
}
