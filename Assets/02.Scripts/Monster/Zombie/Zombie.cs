using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

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

// 좀비 애니메이션 (ZombieAC 컨트롤러의 상태 이름과 같아야 한다)
public enum ZombieAnimation
{
    Idle,
    Walk,
    Run,
    Attack,
    Hit
}

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스텟")]
    [SerializeField] private float _detectDistance = 8f; // 플레이어를 발견하는 거리
    [SerializeField] private float _comebackDistance = 12f; // 플레이어 추적을 포기하는 거리
    [SerializeField] private float _attackRange = 2f; // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f; // 공격 간격
    [SerializeField] private float _attackAnimationTime = 1f; // 공격 애니메이션 길이, 끝나면 Idle로 돌아간다
    [SerializeField] private float _moveSpeed = 5f; // 이동속도
    [SerializeField] private float _maxHealth = 30f; // 최대 체력
    [SerializeField] private float _attackDamage = 10f; // 대미지
    [SerializeField] private float _deathDestroyDelay = 1.5f; // 죽은 뒤 사라지기까지 시간 (쓰러지는 애니메이션보다 길어야 한다)
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
    // private CharacterController _controller;
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

    private NavMeshAgent _agent;

    private Animator _animator;
    private ZombieState _animatedState = ZombieState.Idle; // 마지막으로 애니메이션을 맞춘 상태
    private ZombieAnimation _currentAnimation = ZombieAnimation.Idle; // 지금 재생 중인 애니메이션 (Animator 기본 상태가 Idle)

    private void Awake()
    {
        _currentHealth = _maxHealth;
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        // _controller = GetComponent<CharacterController>();
        // 길을 찾아 걷게 해주는 컴포넌트 NavMeshAgent를 찾아서 참조
        _agent = GetComponent<NavMeshAgent>();
        _startPosition = transform.position;
        _endPosition = _startPosition + _moveDestination;
        _targetPosition = _endPosition;
        _animator = GetComponentInChildren<Animator>();
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

        UpdateAnimation();
    }

    // 상태가 바뀐 프레임에만 그 상태에 맞는 애니메이션으로 전이한다.
    // 상태 전이가 코루틴(Hit 등)에서도 일어나므로 전이 코드마다 넣지 않고 한 곳에서 처리한다.
    private void UpdateAnimation()
    {
        if (_state == _animatedState) return;
        _animatedState = _state;

        ZombieAnimation next;
        switch (_state)
        {
            case ZombieState.Idle:
                next = ZombieAnimation.Idle;
                break;
            case ZombieState.Patrol:
            case ZombieState.Roam:
                next = ZombieAnimation.Walk;
                break;
            case ZombieState.Trace:
            case ZombieState.Comeback:
                next = ZombieAnimation.Run;
                break;
            case ZombieState.Attack:
                next = ZombieAnimation.Idle;
                break;
            case ZombieState.Hit:
                next = ZombieAnimation.Hit;
                break;
            default:
                return;
        }

        PlayAnimation(next);
    }

    private void PlayAnimation(ZombieAnimation next)
    {
        if (next == _currentAnimation) return;

        _animator.SetTrigger($"{_currentAnimation}To{next}");
        _currentAnimation = next;
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

        // transform.forward = direction;
        // _controller.SimpleMove(_moveSpeed * direction);

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

        // Vector3 direction = _roamTarget - transform.position;
        // direction.y = 0;

        // 도착했거나 너무 오래 걸리면(벽에 막힘 등) 잠깐 쉰다.
        bool isArrived = !_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance;
        if (isArrived || _roamTimer >= _roamTimeout)
        {
            _agent.ResetPath();
            _waitTimer = UnityEngine.Random.Range(_roamWaitRange.x, _roamWaitRange.y);
            GameLogger.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;
            return;
        }

        // direction.Normalize();
        // transform.forward = direction;
        // _controller.SimpleMove(_roamSpeed * direction);

        CheckDetectPlayer();
    }

    private void StartRoam()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * _roamRadius;
        Vector3 candidate = _startPosition + new Vector3(offset.x, 0f, offset.y);
        if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            _waitTimer = UnityEngine.Random.Range(_roamWaitRange.x, _roamWaitRange.y);
            _state = ZombieState.Idle;
            return;
        }

        _roamTarget = hit.position;
        _roamTimer = 0f;
        _state = ZombieState.Roam;

        _agent.speed = _roamSpeed;
        _agent.SetDestination(_roamTarget);
    }

    // 플레이어가 감지 거리 안에 있으면 Trace 상태로 전이한다.
    private void CheckDetectPlayer()
    {
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _detectDistance)
        {
            GameLogger.Log($"{_state} -> Trace");
            _agent.speed = _moveSpeed;
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

            // 앞으로 쓰러질지 뒤로 쓰러질지 랜덤으로 고른다. (Any State에서 바로 전이되는 트리거)
            _animator.SetTrigger(UnityEngine.Random.value < 0.5f ? "DieForward" : "DieBack");

            StartCoroutine(Death_Coroutine());
            OnDeath?.Invoke();
        }

        OnDamaged?.Invoke();
    }

    private IEnumerator Hit_Coroutine()
    {
        StopAgent();
        yield return new WaitForSeconds(0.3f);
        GameLogger.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private void StopAgent()
    {
        _agent.ResetPath(); // 에이전트의 목적지 삭제
        _agent.velocity = Vector3.zero; // 미끄럼 방지를 위해 가속도 제로
    }

    private IEnumerator Death_Coroutine()
    {
        StopAgent();
        _agent.enabled = false;
        yield return new WaitForSeconds(_deathDestroyDelay);
        Destroy(gameObject);
    }

    private void Idle()
    {
        StopAgent();
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

        // Vector3 direction = _player.transform.position - transform.position;
        // direction.y = 0;
        // direction.Normalize();

        // transform.forward = direction;
        // _controller.SimpleMove(_moveSpeed * direction);

        _agent.SetDestination(_player.transform.position);
    }

    private void Comeback()
    {
        // 처음 자리에 도달하면 다시 배회한다.
        // if (Vector3.Distance(transform.position, _startPosition) < _agent.stoppingDistance)
        // if (_agent.remainingDistance <= _agent.stoppingDistance)
        if (_agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            GameLogger.Log($"{_state} -> Roam");
            StartRoam();
            return;
        }

        // 처음 자리로 되돌아감
        // Vector3 direction = _startPosition - transform.position;
        // direction.y = 0;
        // direction.Normalize();
        // transform.forward = direction;
        // _controller.SimpleMove(_moveSpeed * direction);
        _agent.SetDestination(_startPosition);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        StopAgent();
        // 플레이어가 공격 사정 거리 밖으로 나가면 Trace
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance > _attackRange)
        {
            GameLogger.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        _attackTimer += Time.deltaTime;

        // 공격 모션이 끝나면 다음 공격 딜레이 동안 Idle로 기다린다.
        if (_currentAnimation == ZombieAnimation.Attack && _attackTimer >= _attackAnimationTime)
        {
            PlayAnimation(ZombieAnimation.Idle);
        }

        if (_attackTimer >= _attackInterval)
        {
            // 공격 딜레이가 끝나면 한 번 공격한다.
            PlayAnimation(ZombieAnimation.Attack);
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
