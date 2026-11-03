using System.Collections;
using UnityEngine;

// 씬이 시작되면 검은 화면에서 서서히 밝아지게 한다.
[RequireComponent(typeof(CanvasGroup))]
public class UI_SceneFade : MonoBehaviour
{
    [Header("페이드 인")]
    [SerializeField] private float _startDelay = 0.5f;
    [SerializeField] private float _fadeDuration = 2f;

    // 로딩 끊김으로 한 프레임 시간이 튀어도 페이드가 건너뛰지 않도록 제한한다.
    private const float MaxFrameDelta = 1f / 30f;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // 에디터에서는 작업하기 편하게 투명하게 두고, 실행하면 검은 화면으로 시작한다.
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true; // 페이드 중에는 클릭을 막는다.
    }

    private void Start()
    {
        StartCoroutine(FadeIn_Coroutine());
    }

    private IEnumerator FadeIn_Coroutine()
    {
        // 1. 첫 프레임 로딩 끊김이 지나가도록 잠깐 기다린다. 끊긴 시간은 세지 않는다.
        float waitTime = 0f;
        while (waitTime < _startDelay)
        {
            waitTime += Mathf.Min(Time.unscaledDeltaTime, MaxFrameDelta);
            yield return null;
        }

        // 2. 검은 화면을 서서히 투명하게 만든다.
        float elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Mathf.Min(Time.unscaledDeltaTime, MaxFrameDelta);
            float t = Mathf.Clamp01(elapsedTime / _fadeDuration);
            _canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, t);
            yield return null;
        }

        // 3. 끝나면 클릭을 막지 않도록 끈다.
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
