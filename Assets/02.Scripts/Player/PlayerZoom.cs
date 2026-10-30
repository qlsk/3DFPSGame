using UnityEngine;

// 저격 모드에서 마우스 오른쪽 버튼을 누르고 있는 동안 시야각을 좁혀 확대한다.
public class PlayerZoom : MonoBehaviour
{
    [SerializeField] private float _normalFov = 60f; // 평소 시야각
    [SerializeField] private float _zoomFov = 15f; // 확대했을 때 시야각

    [SerializeField] private GameObject _sniperZoom;
    private bool _isZooming = false;
    [SerializeField] private float _zoomDelay = 1f;
    private float _zoomTimer = 0f;
    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        _zoomTimer += Time.deltaTime;
        if (_zoomTimer < _zoomDelay) return;

        if (Input.GetMouseButton(1))
        {
            _isZooming = !_isZooming;
            _zoomTimer = 0f;
        }

        bool isZooming = GameManager.Instance.State == GameState.Playing
                         && _player.WeaponMode == WeaponMode.Sniper
                         && _isZooming;

        Camera.main.fieldOfView = isZooming ? _zoomFov : _normalFov;
        _sniperZoom.SetActive(isZooming);
    }
}
