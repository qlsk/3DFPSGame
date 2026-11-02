using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// 플레이어가 맞으면 화면 가장자리를 붉게 하고 색을 갈랐다가 서서히 되돌린다.
public class PostHitEffect : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private Volume _volume;

    [Header("피격 연출")]
    [SerializeField] private Color _hitColor = new Color(0.6f, 0f, 0f);
    [SerializeField] private float _hitVignette = 0.5f;
    [SerializeField] private float _hitChromatic = 0.6f;
    [SerializeField] private float _fadeTime = 0.4f;

    private Vignette _vignette;
    private ChromaticAberration _chromatic;

    // 평소 값: 효과가 끝나면 이 값으로 돌아온다.
    private Color _baseColor;
    private float _baseVignette;
    private float _baseChromatic;

    private Coroutine _fadeCoroutine;
    private Player _player;

    private void Start()
    {
        // 1. profile은 Play 중에만 쓰는 복사본이라, 값을 바꿔도 에셋은 그대로다.
        if (_volume == null ||
            !_volume.profile.TryGet(out _vignette) ||
            !_volume.profile.TryGet(out _chromatic))
        {
            GameLogger.LogWarning("Volume과 Profile의 Vignette, Chromatic Aberration을 확인하세요.");
            enabled = false;
            return;
        }

        // 2. 평소 값을 기억해 둔다.
        _baseColor = _vignette.color.value;
        _baseVignette = _vignette.intensity.value;
        _baseChromatic = _chromatic.intensity.value;

        // 3. 플레이어가 맞으면 효과를 낸다.
        _player = FindAnyObjectByType<Player>();
        if (_player != null)
        {
            _player.OnDamaged += OnPlayerDamaged;
        }
    }

    private void OnDestroy()
    {
        if (_player != null)
        {
            _player.OnDamaged -= OnPlayerDamaged;
        }
    }

    private void OnPlayerDamaged()
    {
        // 연달아 맞으면 하던 것을 멈추고 처음부터 다시 한다.
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(Fade_Coroutine());
    }

    private IEnumerator Fade_Coroutine()
    {
        _vignette.color.Override(_hitColor);

        float elapsedTime = 0f;
        while (elapsedTime < _fadeTime)
        {
            float t = elapsedTime / _fadeTime;
            _vignette.intensity.Override(Mathf.Lerp(_hitVignette, _baseVignette, t));
            _chromatic.intensity.Override(Mathf.Lerp(_hitChromatic, _baseChromatic, t));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _vignette.color.Override(_baseColor);
        _vignette.intensity.Override(_baseVignette);
        _chromatic.intensity.Override(_baseChromatic);
        _fadeCoroutine = null;
    }
}
