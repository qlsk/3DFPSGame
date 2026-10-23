// "데미지를 받을 수 있다"라는 약속(인터페이스)
using UnityEngine;

public interface IDamageable
{
    // 이 약속은 아래 메서드를 구현해야 한다는 규칙을 가지고 있다.
    void TakeDamage(DamageInfo damageInfo);
}
