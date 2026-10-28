using UnityEngine;

// Animator가 붙은 자식 오브젝트에서 애니메이션 이벤트를 받아 부모의 플레이어 컴포넌트로 전달한다.
public class PlayerAnimationEvents : MonoBehaviour
{
    private PlayerBombFire _bombFire;

    private void Awake()
    {
        _bombFire = GetComponentInParent<PlayerBombFire>();
    }

    // Throwing 클립의 애니메이션 이벤트: 폭탄이 손을 떠나는 프레임
    public void OnThrowRelease()
    {
        _bombFire.ReleaseBomb();
    }
}
