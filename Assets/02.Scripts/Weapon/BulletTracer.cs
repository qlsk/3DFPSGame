using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private Color _color;
    [SerializeField] private float _duration = 0.06f;

    private float _remaining;

    private void Start()
    {
        _color = _line.startColor;
        _line.gameObject.SetActive(false);
    }

    public void Show(Vector3 start, Vector3 end)
    {
        _line.SetPosition(0, start);
        _line.SetPosition(1, end);
        SetAlpha(1f);

        _remaining = _duration;
        _line.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!_line.gameObject.activeSelf) return;

        _remaining -= Time.deltaTime;

        if (_remaining <= 0f)
        {
            _line.gameObject.SetActive(false);
            return;
        }

        SetAlpha(_remaining / _duration);
    }

    private void SetAlpha(float alpha)
    {
        Color color = _color;
        color.a *= alpha;
        _line.startColor = color;
        _line.endColor = color;
    }
}
