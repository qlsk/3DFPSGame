using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던진다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;

    [SerializeField] private float _minThrowPower = 15f;
    public float MinThrowPower => _minThrowPower;
    [SerializeField] private float _maxThrowPower = 90f;
    public float MaxThrowPower => _maxThrowPower;
    [SerializeField] private float _throwPowerUpRate = 15f;
    private float _currentThrowPower = 15f;
    public float CurrentThrowPower => _currentThrowPower;

    [SerializeField] private int _maxThrowCount = 10;
    public int MaxThrowCount => _maxThrowCount;
    private int _currentThrowCount = 0;
    public int CurrentThrowCount => _currentThrowCount;

    private bool _isCharging = false;
    public bool IsCharging => _isCharging;

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // 1. 마우스 오른쪽 버튼 입력 받는 동안 Throw Power 상승
        if (Input.GetButton("Fire2") && _currentThrowCount < _maxThrowCount && _currentThrowPower <= _maxThrowPower)
        {
            _currentThrowPower = Mathf.Min(_currentThrowPower + _throwPowerUpRate * Time.deltaTime, _maxThrowPower);
            // GameLogger.Log(_currentThrowPower);
            _isCharging = true;
        }

        // 1-2. 마우스 오른쪽 버튼 입력이 없어지면 발사
        if (Input.GetButtonUp("Fire2") && _currentThrowCount < _maxThrowCount)
        {
            Shoot();
            _currentThrowPower = _minThrowPower;
            _isCharging = false;
        }
    }

    private void Shoot()
    {
        _currentThrowCount++;
        // 2. 발사 위치에서 수류탄 생성하기
        Bomb bomb = Instantiate(_bombPrefab, _firePoint.position, Quaternion.identity);

        // 3. 생성된 수류탄을 시선 방향으로 던지기
        Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.main.transform.forward * _currentThrowPower, ForceMode.Impulse);
    }
}
