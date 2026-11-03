using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Playing,
    Pause,
    GameOver
}

public class GameManager : SingletonBehaviour<GameManager>
{
    private GameState _state = GameState.Ready;
    public GameState State => _state;

    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private GameObject _optionPopup;
    [SerializeField] private GameObject _gameOverPopup;

    private void Start()
    {
        StartCoroutine(StartSequence_Coroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == GameState.Playing)
            {
                _state = GameState.Pause;
                Time.timeScale = 0; // 유니티에서 흐르는 게임 시간
                _optionPopup.SetActive(true);
                CursorManager.Instance.UnlockCursor();
            }
            else if (_state == GameState.Pause)
            {
                Resum();
            }
        }
    }

    private IEnumerator StartSequence_Coroutine()
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = "준비";
        yield return new WaitForSeconds(1.5f);
        _stateText.text = "시작";
        yield return new WaitForSeconds(0.5f);
        _state = GameState.Playing;
        _stateText.gameObject.SetActive(false);
    }

    public void GameOver()
    {
        // 1. 게임 오버 상태로 바꿔 입력과 AI를 멈춘다.
        _state = GameState.GameOver;
        Time.timeScale = 0;

        // 2. 팝업을 띄우고 버튼을 누를 수 있게 커서를 해제한다.
        _gameOverPopup.SetActive(true);
        CursorManager.Instance.UnlockCursor();
    }

    // 계속하기
    public void Resum()
    {
        _state = GameState.Playing;
        Time.timeScale = 1f;
        _optionPopup.SetActive(false);
        CursorManager.Instance.LockCursor();
    }

    // 다시하기
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 종료
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
