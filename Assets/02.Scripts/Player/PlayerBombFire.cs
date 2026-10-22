using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 마우스 오른쪽 버튼을 누르면 시선이 바라보는 방향으로 수류탄을 던진다.
public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Bomb _bombPrefab;

    [SerializeField] private float _minThrowPower = 15f;
    [SerializeField] private float _maxThrowPower = 90f;
    [SerializeField] private float _throwPowerUpRate = 15f;
    private float _currentThrowPower = 15f;

    [SerializeField] private int _maxThrowCount = 10;
    private int _currentThrowCount = 0;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _throwCountText;
    [SerializeField] private Slider _throwPowerSlider;

    private void Start()
    {
        _throwPowerSlider.maxValue = _maxThrowPower;
        _throwPowerSlider.minValue = _minThrowPower;
        UpdateThrowPowerSlider();
        UpdateThrowCountText();
    }

    private void Update()
    {
        // 1. 마우스 오른쪽 버튼 입력 받는 동안 Throw Power 상승
        if (Input.GetButton("Fire2") && _currentThrowCount < _maxThrowCount && _currentThrowPower <= _maxThrowPower)
        {
            _currentThrowPower = Mathf.Min(_currentThrowPower + _throwPowerUpRate * Time.deltaTime, _maxThrowPower);
            // GameLogger.Log(_currentThrowPower);
            _throwPowerSlider.gameObject.SetActive(true);
            UpdateThrowPowerSlider();
        }

        // 1-2. 마우스 오른쪽 버튼 입력이 없어지면 발사
        if (Input.GetButtonUp("Fire2") && _currentThrowCount < _maxThrowCount)
        {
            Shoot();
            _currentThrowPower = _minThrowPower;
            _throwPowerSlider.gameObject.SetActive(false);
            UpdateThrowCountText();
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

    private void UpdateThrowPowerSlider()
    {
        _throwPowerSlider.value = _currentThrowPower;
    }

    private void UpdateThrowCountText()
    {
        _throwCountText.text = $"{_maxThrowCount - _currentThrowCount}/{_maxThrowCount}";
    }
}
