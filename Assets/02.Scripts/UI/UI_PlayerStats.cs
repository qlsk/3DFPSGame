using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    private Player _player;
    private PlayerGunFire _playerGunFire;
    private PlayerBombFire _playerBombFire;
    private PlayerMove _playerMove;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _bulletCountSlider;
    [SerializeField] private Slider _playerStaminaSlider;
    [SerializeField] private TextMeshProUGUI _throwCountText;
    [SerializeField] private Slider _throwPowerSlider;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _playerGunFire = GetComponent<PlayerGunFire>();
        _playerMove = GetComponent<PlayerMove>();
        _playerBombFire = GetComponent<PlayerBombFire>();
    }

    private void Start()
    {
        _healthSlider.maxValue = _player.MaxHealth;
        _bulletCountSlider.maxValue = _playerGunFire.MaxBulletCount;
        _playerStaminaSlider.maxValue = _playerMove.MaxStamina;
        _playerStaminaSlider.minValue = _playerMove.SprintCost;
        _throwPowerSlider.maxValue = _playerBombFire.MaxThrowPower;
        _throwPowerSlider.minValue = _playerBombFire.MinThrowPower;
    }

    private void LateUpdate()
    {
        _healthSlider.value = _player.CurrentHealth;
        _bulletCountSlider.value = _playerGunFire.BulletCount;
        _playerStaminaSlider.value = _playerMove.Stamina;
        _throwCountText.text =
            $"{_playerBombFire.MaxThrowCount - _playerBombFire.CurrentThrowCount}/{_playerBombFire.MaxThrowCount}";
        if (_playerBombFire.IsCharging)
        {
            _throwPowerSlider.gameObject.SetActive(true);
        }
        else
        {
            _throwPowerSlider.gameObject.SetActive(false);
        }

        _throwPowerSlider.value = _playerBombFire.CurrentThrowPower;
    }
}
