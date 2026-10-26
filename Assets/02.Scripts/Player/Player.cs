using System;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    public float MaxHealth => _maxHealth;
    private float _currentHealth;
    public float CurrentHealth => _currentHealth;

    // Action 자료형: 함수들을 저장할 수 있다.
    public event Action OnDamaged;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentHealth = Mathf.Max(_currentHealth - damageInfo.Value, 0f);


        OnDamaged?.Invoke();

        if (_currentHealth <= 0f)
        {
            GameLogger.Log("플레이어가 쓰러졌다");
            GameManager.Instance.GameOver();
        }
    }
}
