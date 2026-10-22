using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

// 체력이 0이 되면 위로 튀어오르며 폭발하는 드럼통
[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;

    [Header("스텟")]
    [SerializeField] private float _health = 30f;
    [SerializeField] private float _launchForce = 20f;
    [SerializeField] private float _spinPower = 5f;
    [SerializeField] private float _destroyDelay = 10f;

    private Rigidbody _rigidbody;
    private float _currentHealth;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _currentHealth = _health;
    }

    public void Hit(float damage)
    {
        if (_currentHealth <= 0)
        {
            return;
        }

        _currentHealth = Mathf.Clamp(_currentHealth - damage, 0f, _health);

        if (_currentHealth <= 0)
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        // 1. 폭발 이펙트를 생성한다.
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.Play();

        // 2. 위로 튀어오르며 빙글빙글 돈다.
        Vector3 random = Random.insideUnitSphere * _launchForce;
        random.x = Mathf.Abs(random.x);
        random.y = Mathf.Abs(random.y);
        random.z = Mathf.Abs(random.z);

        _rigidbody.AddForce(random, ForceMode.Impulse);
        _rigidbody.AddTorque(Random.insideUnitSphere * _spinPower, ForceMode.Impulse);

        // 3. 사라진다.
        yield return new WaitForSeconds(_destroyDelay);
        Destroy(gameObject);
    }
}
