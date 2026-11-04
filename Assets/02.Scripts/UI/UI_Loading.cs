using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider _loadingSlider;
    [SerializeField] private TextMeshProUGUI _loadingText;

    private void Start()
    {
        StartCoroutine(Load_Coroutine());
    }

    private IEnumerator Load_Coroutine()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        // 비동기 씬 로드
        // 반환: 씬의 비동기 진행사항을 확인할 수 있는 객체
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.NextSceneName);
        // 수많은 객체의 생성을 쪼개서 하나하나 생성할 준비
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            // 0 에서 1 사이의 값으로 보간
            float value = Mathf.Clamp01(operation.progress / 0.9f);

            // 진행률을 로딩바와 텍스트에 표시
            _loadingSlider.value = value;
            _loadingText.text = $"불러오는 중 {Mathf.RoundToInt(value * 100)}%";

            Debug.Log(operation.progress);
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}
