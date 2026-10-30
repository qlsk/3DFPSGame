using UnityEngine;

// 저격 모드에서 마우스 오른쪽 버튼을 누르고 있는 동안 시야각을 좁혀 확대한다.
public class PlayerZoom : MonoBehaviour
{
    [SerializeField] private float _normalFov = 60f; // 평소 시야각
    [SerializeField] private float _zoomFov = 15f; // 확대했을 때 시야각

    private Player _player;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Update()
    {
        bool isZooming = GameManager.Instance.State == GameState.Playing
                         && _player.WeaponMode == WeaponMode.Sniper
                         && Input.GetMouseButton(1);

        Camera.main.fieldOfView = isZooming ? _zoomFov : _normalFov;
    }
}
