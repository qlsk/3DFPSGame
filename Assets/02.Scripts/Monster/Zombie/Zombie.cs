using UnityEngine;

// FSM(유한 상태 머신): 유한개의 상태가 있고, 상태마다 행동을 다르게 하여 AI를 구현하는 유명한 방법
// - "상태": 객체가 가질 수 있는 어떤 조건이나 상황
// - "전이": 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - "머신": 위 방식을 따라 규칙을 세워둔 것

// 좀비가 가질 수 있는 상태
public enum ZombieState
{
    Idle, // 대기: 제자리에서 플레이어를 기다린다.
    Trace, // 추적: 플레이어를 쫓아간다.
    Comeback, // 복귀: 처음 자리로 돌아간다.
    Attack, // 공격: 일정 시간마다 플레이어를 때린다.
    Hit, // 피격: 공격(총알, 폭탄)을 받아 잠깐 멈칫한다.
    Death // 죽음: 스러지고 잠시 뒤 사라진다.
}

public class Zombie : MonoBehaviour
{
    [Header("스텟")]
    [SerializeField] private float _detectDistance = 8; // 플레이어를 발견하는 거리
    [SerializeField] private float _moveSpeed = 5f; // 이동속도
    [SerializeField] private CharacterController _controller;

    private GameObject _player;

    private ZombieState _state = ZombieState.Idle;

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
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

    private void Idle()
    {
        // TODO: Idle 애니메이션 실행
        // 대기: 제자리에서 플레이어를 기다린다.
        // 플레이어와 가까워지면 Trace 상태로 전이한다.
        float distance = Vector3.Distance(transform.position, _player.transform.position);
        if (distance < _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
        }
    }

    private void Trace()
    {
        // TODO: Run 애니메이션 실행
        // 추적: 플레이어를 쫓아간다.
        Vector3 direction = _player.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();
        transform.forward = direction;
        _controller.SimpleMove(_moveSpeed * direction);
    }

    private void Comeback()
    {
    }

    private void Attack()
    {
    }
}
