using UnityEngine;

// 무언가에 부딛히면 폭발 이펙트를 남기고 사라지는 폭탄
public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;

    private void OnCollisionEnter(Collision collision)
    {
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.Play();

        // Todo: 오브젝트 풀링 적용
        // 폭탄을 삭제한다.
        Destroy(gameObject);
    }
}
