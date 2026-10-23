using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    public float MaxHealth => _maxHealth;
    private float _currentHealth;
    public float CurrentHealth => _currentHealth;
    private float _maxColorAlpha = 0.4f;
    [SerializeField] private UI_HitEffect _uiHitEffect;

    private void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _currentHealth = Mathf.Max(_currentHealth - damageInfo.Value, 0f);

        _uiHitEffect.Fade(_maxColorAlpha);

        if (_currentHealth <= 0f)
        {
            GameLogger.Log("플레이어가 쓰러졌다");
            GameManager.Instance.GameOver();
        }
    }
}
