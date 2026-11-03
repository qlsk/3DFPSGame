using UnityEngine;
using UnityEngine.UI;

// 게임 오버 팝업의 버튼 입력을 GameManager에 전달한다.
public class UI_GameOverPopup : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _quitButton;

    private void Awake()
    {
        _restartButton.onClick.AddListener(OnClickRestart);
        _quitButton.onClick.AddListener(OnClickQuit);
    }

    private void OnClickRestart()
    {
        GameManager.Instance.Restart();
    }

    private void OnClickQuit()
    {
        GameManager.Instance.Quit();
    }
}
