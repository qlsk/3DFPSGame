using UnityEngine;
using Lean.Pool;

public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _bombEffectReturnDelay = 3f;

    private bool _hasExploded;

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        ParticleSystem effect = LeanPool.Spawn(_explosionEffectPrefab, transform.position, Quaternion.identity);

        // 이전 재생 상태를 지우고 다시 재생
        effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        effect.Play(true);

        // Bomb가 삭제되어도 Lean Pool이 반환을 처리한다.
        LeanPool.Despawn(effect.gameObject, _bombEffectReturnDelay);

        Destroy(gameObject);
    }
}
