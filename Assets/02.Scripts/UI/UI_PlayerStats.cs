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
        _healthSlider.value = _player.CurrentHealth;
        _player.OnDamaged += RefreshHealth;

        _bulletCountSlider.maxValue = _playerGunFire.MaxBulletCount;
        _bulletCountSlider.value = _playerGunFire.BulletCount;
        _playerGunFire.OnFire += RefreshBulletCount;

        _playerStaminaSlider.maxValue = _playerMove.MaxStamina;
        _playerStaminaSlider.minValue = _playerMove.SprintCost;
        _playerMove.OnStaminaChange += RefreshStamina;

        _throwPowerSlider.maxValue = _playerBombFire.MaxThrowPower;
        _throwPowerSlider.minValue = _playerBombFire.MinThrowPower;
    }

    private void LateUpdate()
    {
        // RefreshHealth(); // 폴링

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

        if (_throwPowerSlider.IsActive())
        {
            _throwPowerSlider.value = _playerBombFire.CurrentThrowPower;
        }
    }

    private void RefreshHealth()
    {
        // 문제: 체력값이 변하지 않았음에도 초당 60회 이상 캔버스를 불필요하게 다시 그린다 -> 성능 낭비가 일어난다.
        // 하나 정도는 괜찮지만, 이런 불필요한 연산이 누적되면 느려진다
        // 해결 : 값이 바뀌었을 때만 캔버스 갱신을 한다.

        // 옵저버 패턴
        // 옵저버 패턴은 한 객체(주체)의 상태가 바뀔 때 이를 구독 중인 여러 객체(관찰자)에게 자동으로 알려주는 패턴으로, 서로 직접 참조하지 않아 결합도가 낮아진다.
        // 유튜브 구독 패턴은 유튜버가 영상을 올릴 때 이를 구독 중인 여러 구독자에게
        // C#에서는 이것을 Action으로 구현할 수 있다.
        _healthSlider.value = _player.CurrentHealth;
    }

    private void RefreshBulletCount()
    {
        _bulletCountSlider.value = _playerGunFire.BulletCount;
    }

    private void RefreshStamina()
    {
        _playerStaminaSlider.value = _playerMove.Stamina;
    }
}
