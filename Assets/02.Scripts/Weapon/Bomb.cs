using UnityEngine;
using Lean.Pool;

public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _explosionRange = 4.5f;
    [SerializeField] private float _damage = 50f;
    [SerializeField] private float _bombEffectReturnDelay = 3f;

    private bool _hasExploded;

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        ParticleSystem effect = LeanPool.Spawn(_explosionEffectPrefab, transform.position, Quaternion.identity);

        if (effect != null)
        {
            // 이전 재생 상태를 지우고 다시 재생
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            effect.Play(true);

            // Bomb가 삭제되어도 Lean Pool이 반환을 처리한다.
            LeanPool.Despawn(effect.gameObject, _bombEffectReturnDelay);
        }

        ApplyAreaDamage();

        Destroy(gameObject);
    }

    private void ApplyAreaDamage()
    {
        // 0. 주위에 있는 드럼통들을 모두 찾는다.
        // Physics.OverlapSphere(원점, 반지름, 찾을 레이어들)
        Collider[] colliders =
            Physics.OverlapSphere(transform.position, _explosionRange, LayerMask.GetMask("Drum", "Monster"));
        // 1. 좀비들과 드럼통들에게 데미지를 준다.
        foreach (Collider col in colliders)
        {
            col.GetComponent<IDamageable>()?.TakeDamage(_damage);
        }
    }
}
