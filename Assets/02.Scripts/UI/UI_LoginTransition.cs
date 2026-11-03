using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 로그인 화면과 회원가입 화면을 옆으로 밀어내며 전환한다.
public class UI_LoginTransition : MonoBehaviour
{
    [Header("그룹")]
    [SerializeField] private CanvasGroup _loginGroup;
    [SerializeField] private CanvasGroup _signUpGroup;

    [Header("버튼")]
    [SerializeField] private Button _openSignUpButton;
    [SerializeField] private Button _backButton;

    [Header("연출")]
    [SerializeField] private float _fadeOutDuration = 0.15f;
    [SerializeField] private float _fadeInDuration = 0.25f;
    [SerializeField] private float _slideDistance = 80f;

    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        _openSignUpButton.onClick.AddListener(OnClickOpenSignUp);
        _backButton.onClick.AddListener(OnClickBack);

        // 시작은 항상 로그인 화면이다.
        ShowImmediately(_loginGroup);
        HideImmediately(_signUpGroup);
    }

    private void OnClickOpenSignUp()
    {
        Switch(_loginGroup, _signUpGroup, 1f);
    }

    private void OnClickBack()
    {
        Switch(_signUpGroup, _loginGroup, -1f);
    }

    // direction: 1이면 새 화면이 오른쪽에서 들어오고, -1이면 왼쪽에서 들어온다.
    private void Switch(CanvasGroup from, CanvasGroup to, float direction)
    {
        // 전환 중에는 버튼이 막혀 있지만, 코드에서 연달아 호출해도 겹치지 않게 한다.
        if (_transitionCoroutine != null)
        {
            return;
        }

        _transitionCoroutine = StartCoroutine(Transition_Coroutine(from, to, direction));
    }

    private IEnumerator Transition_Coroutine(CanvasGroup from, CanvasGroup to, float direction)
    {
        RectTransform fromRect = (RectTransform)from.transform;
        RectTransform toRect = (RectTransform)to.transform;
        Vector2 outPosition = new Vector2(-direction * _slideDistance, 0f);
        Vector2 inStartPosition = new Vector2(direction * _slideDistance, 0f);

        // 1. 전환하는 동안 두 화면 모두 클릭을 막는다.
        SetInteractable(from, false);
        SetInteractable(to, false);

        // 2. 현재 화면을 밀어내며 사라지게 한다.
        float elapsedTime = 0f;
        while (elapsedTime < _fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = EaseOut(elapsedTime / _fadeOutDuration);
            from.alpha = 1f - t;
            fromRect.anchoredPosition = Vector2.Lerp(Vector2.zero, outPosition, t);
            yield return null;
        }

        HideImmediately(from);

        // 3. 새 화면을 반대쪽에서 들어오며 나타나게 한다.
        to.gameObject.SetActive(true);
        to.alpha = 0f;
        toRect.anchoredPosition = inStartPosition;

        elapsedTime = 0f;
        while (elapsedTime < _fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = EaseOut(elapsedTime / _fadeInDuration);
            to.alpha = t;
            toRect.anchoredPosition = Vector2.Lerp(inStartPosition, Vector2.zero, t);
            yield return null;
        }

        ShowImmediately(to);
        _transitionCoroutine = null;
    }

    private void ShowImmediately(CanvasGroup group)
    {
        group.gameObject.SetActive(true);
        group.alpha = 1f;
        ((RectTransform)group.transform).anchoredPosition = Vector2.zero;
        SetInteractable(group, true);
    }

    private void HideImmediately(CanvasGroup group)
    {
        // 다음에 다시 보일 때를 위해 위치는 원래대로 돌려 두고 끈다.
        group.alpha = 0f;
        ((RectTransform)group.transform).anchoredPosition = Vector2.zero;
        SetInteractable(group, false);
        group.gameObject.SetActive(false);
    }

    private void SetInteractable(CanvasGroup group, bool isInteractable)
    {
        group.interactable = isInteractable;
        group.blocksRaycasts = isInteractable;
    }

    // 처음엔 빠르고 끝에서 부드럽게 멈춘다.
    private float EaseOut(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t) * (1f - t);
    }
}
