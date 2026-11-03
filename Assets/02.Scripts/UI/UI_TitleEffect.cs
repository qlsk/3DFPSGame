using System.Collections;
using TMPro;
using UnityEngine;

// 제목이 위에서 내려오며 나타나고, 이후 몇 초마다 짧게 흔들리며 붉게 번쩍인다.
[RequireComponent(typeof(TMP_Text))]
public class UI_TitleEffect : MonoBehaviour
{
    [Header("등장")]
    [SerializeField] private float _introDelay = 1.2f;
    [SerializeField] private float _introDuration = 1f;
    [SerializeField] private float _introOffsetY = 40f;
    [SerializeField] private float _introStartScale = 1.15f;

    [Header("글리치")]
    [SerializeField] private float _glitchIntervalMin = 3f;
    [SerializeField] private float _glitchIntervalMax = 6f;
    [SerializeField] private float _glitchDuration = 0.25f;
    [SerializeField] private float _glitchStrength = 8f;
    [SerializeField] private float _glitchStepTime = 0.04f;
    [SerializeField] private Color _glitchColor = new Color(1f, 0.15f, 0.15f, 1f);

    [Header("번개")]
    [SerializeField] private UI_LightningEffect _lightning;

    // 로딩 끊김으로 한 프레임 시간이 튀어도 등장 연출이 건너뛰지 않도록 제한한다.
    private const float MaxFrameDelta = 1f / 30f;

    private TMP_Text _text;
    private RectTransform _rect;
    private Vector2 _basePosition;
    private Vector3 _baseScale;
    private Color _baseColor;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _rect = (RectTransform)transform;
        _basePosition = _rect.anchoredPosition;
        _baseScale = _rect.localScale;
        _baseColor = _text.color;

        // 등장 전에는 보이지 않게 한다.
        SetAlpha(0f);
    }

    private void Start()
    {
        StartCoroutine(Intro_Coroutine());
    }

    private void OnDisable()
    {
        // 중간에 꺼져도 흔들린 상태로 남지 않게 되돌린다.
        _rect.anchoredPosition = _basePosition;
        _rect.localScale = _baseScale;
        _text.color = _baseColor;
    }

    private IEnumerator Intro_Coroutine()
    {
        // 1. 화면 페이드 인이 어느 정도 진행될 때까지 기다린다. 끊긴 시간은 세지 않는다.
        float waitTime = 0f;
        while (waitTime < _introDelay)
        {
            waitTime += Mathf.Min(Time.unscaledDeltaTime, MaxFrameDelta);
            yield return null;
        }

        // 2. 위에서 내려오며 커졌던 크기가 줄고, 서서히 나타난다.
        Vector2 startPosition = _basePosition + new Vector2(0f, _introOffsetY);
        float elapsedTime = 0f;
        while (elapsedTime < _introDuration)
        {
            elapsedTime += Mathf.Min(Time.unscaledDeltaTime, MaxFrameDelta);
            float t = EaseOut(elapsedTime / _introDuration);
            _rect.anchoredPosition = Vector2.Lerp(startPosition, _basePosition, t);
            _rect.localScale = _baseScale * Mathf.Lerp(_introStartScale, 1f, t);
            SetAlpha(t);
            yield return null;
        }

        _rect.anchoredPosition = _basePosition;
        _rect.localScale = _baseScale;
        _text.color = _baseColor;

        // 3. 등장이 끝나면 글리치를 반복한다.
        StartCoroutine(GlitchLoop_Coroutine());
    }

    private IEnumerator GlitchLoop_Coroutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(_glitchIntervalMin, _glitchIntervalMax));
            yield return Glitch_Coroutine();
        }
    }

    private IEnumerator Glitch_Coroutine()
    {
        // 깜빡임이 시작되는 순간 번개를 친다.
        if (_lightning != null)
        {
            _lightning.Strike();
        }

        // 짧은 간격마다 위치와 색을 무작위로 바꿔 화면이 튀는 느낌을 준다.
        float elapsedTime = 0f;
        while (elapsedTime < _glitchDuration)
        {
            Vector2 offset = new Vector2(Random.Range(-1f, 1f) * _glitchStrength, Random.Range(-0.3f, 0.3f) * _glitchStrength);
            _rect.anchoredPosition = _basePosition + offset;
            _text.color = Random.value < 0.5f ? _glitchColor : _baseColor;

            yield return new WaitForSecondsRealtime(_glitchStepTime);
            elapsedTime += _glitchStepTime;
        }

        _rect.anchoredPosition = _basePosition;
        _text.color = _baseColor;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _baseColor;
        color.a = _baseColor.a * alpha;
        _text.color = color;
    }

    // 처음엔 빠르고 끝에서 부드럽게 멈춘다.
    private float EaseOut(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }
}
