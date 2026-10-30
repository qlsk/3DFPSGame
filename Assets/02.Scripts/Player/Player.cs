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

    private Animator _animator;
    private int _injuredIndex;

    private WeaponMode _weaponMode = WeaponMode.Normal;
    public WeaponMode WeaponMode => _weaponMode;

    public event Action<WeaponMode> OnWeaponModeChanged;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeWeaponMode(WeaponMode.Normal);
        }

        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeWeaponMode(WeaponMode.Sniper);
        }
    }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _injuredIndex = _animator.GetLayerIndex("Injured Layer");
        _currentHealth = _maxHealth;
    }

    private void ChangeWeaponMode(WeaponMode weaponMode)
    {
        _weaponMode = weaponMode;
        OnWeaponModeChanged?.Invoke(weaponMode);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentHealth = Mathf.Max(_currentHealth - damageInfo.Value, 0f);

        // 체력에 따라 Injured Layer의 가중치를 바꿔준다.
        float healthPercent = _currentHealth / _maxHealth; // 0 ~ 1
        float injured = Mathf.InverseLerp(0.5f, 0f, healthPercent);
        _animator.SetLayerWeight(_injuredIndex, injured);
        OnDamaged?.Invoke();

        if (_currentHealth <= 0f)
        {
            gameObject.GetComponentInChildren<Animator>().SetFloat("MoveMotion", 0f);
            GameLogger.Log("플레이어가 쓰러졌다");
            GameManager.Instance.GameOver();
        }
    }
}
