using UnityEngine;

// 키보드 방향키에 따라 그 방향으로 이동
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다
        // GetAxis = -1 ~ 1
        // GetAxisRaw = -1, 0, 1
        var h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        var v = Input.GetAxisRaw("Vertical"); // -1, 0, 1

        // 2. 키보드 입력에 따라 방향을 구한다.
        var direction = new Vector3(h, 0, v);
        direction.Normalize(); // 대각선이어도 방향의 길이를 1로 바꾸기 위해
        direction = transform.TransformDirection(direction);
        // 로컬 좌표계: 한 대상을 중심
        // 월드 좌표계: 세상을 중심

        // 3. 방향과 속력에 따라 이동한다.
        // transform.position += _moveSpeed * Time.deltaTime * direction;
        _characterController.Move(_moveSpeed * Time.deltaTime * direction);
    }
}
