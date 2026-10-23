using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;

    private Zombie _zombie;
    // private GameObject _player;

    private void Awake()
    {
        // _player = GameObject.FindGameObjectWithTag("Player");
        _zombie = GetComponent<Zombie>();
    }

    private void Start()
    {
        _hpSlider.maxValue = _zombie.MaxHealth;
        _hpSlider.value = _zombie.CurrentHealth;
    }

    private void LateUpdate()
    {
        // 빌보드: 체력바가 카메라를 쳐다보게 한다.
        _hpSlider.transform.forward = Camera.main.transform.forward;
        // _hpSlider.transform.LookAt(_player.transform);
        _hpSlider.value = _zombie.CurrentHealth;
    }
}
