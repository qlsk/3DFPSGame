using UnityEngine;

// TODO: 공격 받으면 산산조각
public class Box : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 20f;
    private float _currentHealth;

    public void TakeDamage(float damage)
    {
        _currentHealth = Mathf.Max(_currentHealth - damage, 0f);
    }

    private void Start()
    {
    }

    private void Update()
    {
    }
}
