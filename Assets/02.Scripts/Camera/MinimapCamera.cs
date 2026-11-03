using UnityEngine;

// 미니맵 카메라: 플레이어 머리 위에서 아래를 내려다보며 따라다닌다.
public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _height = 8f;
    [SerializeField] private int _maxOrthographicSize = 10;
    [SerializeField] private float _minOrthographicSize = 4;

    [Header("렌더링")]
    // 미니맵을 몇 프레임마다 다시 그릴지 정한다. 1 이하이면 카메라가 매 프레임 알아서 그린다.
    [SerializeField] private int _renderInterval = 3;

    private Camera _camera;
    private int _frameCounter;
    private bool _renderRequested = true;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographicSize = _maxOrthographicSize;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ZoomIn();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ZoomOut();
        }
    }

    private void LateUpdate()
    {
        // 1. 대상의 머리 위로 위치를 옮긴다.
        transform.position = _target.position + (Vector3.up * _height);

        // 2. 아래를 보되, 대상이 쳐다보는 방향으로 카메라도 회전한다.
        transform.eulerAngles = new Vector3(90, _target.eulerAngles.y, 0f);

        // 3. 간격이 1 이하이면 카메라 자동 렌더링에 맡기고, 아니면 직접 그린다.
        bool autoRender = _renderInterval <= 1;
        _camera.enabled = autoRender;
        if (autoRender) return;

        // 4. 줌이 바뀌었거나 간격이 지났을 때만 한 번 그린다.
        _frameCounter++;
        if (_renderRequested || _frameCounter >= _renderInterval)
        {
            _camera.Render();
            _frameCounter = 0;
            _renderRequested = false;
        }
    }

    public void ZoomIn()
    {
        _camera.orthographicSize = Mathf.Max(_minOrthographicSize, _camera.orthographicSize - 1);
        _renderRequested = true;
    }

    public void ZoomOut()
    {
        _camera.orthographicSize = Mathf.Min(_maxOrthographicSize, _camera.orthographicSize + 1);
        _renderRequested = true;
    }
}
