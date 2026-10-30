using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerWeapons : MonoBehaviour
{
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private Sprite[] _modeIcons;
    [SerializeField] private TextMeshProUGUI _bulletText;
    [SerializeField] private TextMeshProUGUI _bombText;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerGunFire _playerGunFire;
    [SerializeField] private PlayerBombFire _playerBombFire;

    private void OnEnable()
    {
        _player.OnWeaponModeChanged += RefreshIcon;
        _playerGunFire.OnBulletCountChanged += RefreshBullet;
        _playerBombFire.OnBombCountChanged += RefreshBomb;
    }

    private void OnDisable()
    {
        _player.OnWeaponModeChanged -= RefreshIcon;
        _playerGunFire.OnBulletCountChanged -= RefreshBullet;
        _playerBombFire.OnBombCountChanged -= RefreshBomb;
    }

    private void Start()
    {
        RefreshIcon(_player.WeaponMode);
        RefreshBullet(_playerGunFire.BulletCount, _playerGunFire.MaxBulletCount);
        RefreshBomb(_playerBombFire.MaxThrowCount - _playerBombFire.CurrentThrowCount);
    }

    private void RefreshIcon(WeaponMode mode)
    {
        _weaponIcon.sprite = _modeIcons[(int)mode];
    }

    private void RefreshBullet(int count, int maxCount)
    {
        _bulletText.text = $"{count} / {maxCount}";
    }

    private void RefreshBomb(int count)
    {
        _bombText.text = $"폭탄 {count}";
    }
}
