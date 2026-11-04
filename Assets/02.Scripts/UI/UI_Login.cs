using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Login : MonoBehaviour
{
    private enum Mode
    {
        Login,
        Register,
    }

    [Header("입력 칸")]
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _passwordConfirmInput;

    [Header("버튼")]
    [SerializeField] private Button _loginButton; // 로그인 모드: 로그인
    [SerializeField] private Button _gotoRegisterButton; // 로그인 모드: 회원가입 모드로
    [SerializeField] private Button _registerButton; // 회원가입 모드: 가입하기
    [SerializeField] private Button _gotoLoginButton; // 회원가입 모드: 로그인 모드로

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _messageText; // 안내 글자
    [SerializeField] private string _gameSceneName = "GameScene"; // 로그인하면 불러올 씬

    // 아이디: 이메일 형식이거나, 영문·숫자·-·@·_로 이루어진 3~20 글자
    private const string IdPattern = @"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}|[A-Za-z0-9\-@_]{3,20})$";

    // 비밀번호: 공백 없는 영문·숫자·기호 8~30 글자, 대문자·소문자·숫자·기호를 각각 하나 이상 포함
    // (?=.*X)는 전방 탐색으로, 문자열 어딘가에 X가 있는지만 확인하고 위치는 옮기지 않는다.
    private const string PasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[\x21-\x7E]{8,30}$";

    private PlayerPrefsAccountRepository _repository = new PlayerPrefsAccountRepository();

    private Mode _mode = Mode.Login;

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isLoginMode = _mode == Mode.Login;
        _passwordConfirmInput.gameObject.SetActive(!isLoginMode);

        _loginButton.gameObject.SetActive(isLoginMode);
        _gotoRegisterButton.gameObject.SetActive(isLoginMode);

        _registerButton.gameObject.SetActive(!isLoginMode);
        _gotoLoginButton.gameObject.SetActive(!isLoginMode);
    }

    public void GotoLogin()
    {
        _mode = Mode.Login;
        Refresh();
    }

    public void GotoRegister()
    {
        _mode = Mode.Register;
        Refresh();
    }

    public void Login()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        if (_repository.CheckPassword(id, password))
        {
            SceneManager.LoadScene(_gameSceneName);
        }
        else
        {
            _messageText.text = "아이디와 비밀번호를 확인해주세요.";
            return;
        }
    }

    public void Register()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;
        string passwordConfirm = _passwordConfirmInput.text;

        // 정규 표현식(정규식)
        // 문자열에서 특정한 규칙이나 패턴을 찾기 위해 사용하는 문법
        // - 데이터 검증(이메일 형식, 주민번호 형식, 비밀번호 형식)
        // - 문자열 검색(TODO라는게 있냐?, 한국어가 있냐?...)
        // - 문자열 치환(규칙에 맞는 문자열을 찾아서 다른 문자열로 변경)
        // - 웹 크롤링 & 파싱
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        // 1. 아이디 규칙 검사
        if (!Regex.IsMatch(id, IdPattern))
        {
            _messageText.text = "아이디는 이메일 또는 영문, 숫자, -, @, _ 3~20 글자로 입력해주세요.";
            return;
        }

        // 2. 비밀번호 규칙 검사
        if (!Regex.IsMatch(password, PasswordPattern))
        {
            _messageText.text = "비밀번호는 대문자, 소문자, 숫자, 기호를 포함한 8~30 글자로 입력해주세요.";
            return;
        }

        // 3. 비밀번호 확인
        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호가 일치하지 않습니다.";
            return;
        }

        // TODO: 중복 ID 검사
        // TODO: 실제 회원가입 데이터 저장

        if (_repository.Exists(id))
        {
            _messageText.text = "이미 존재하는 아이디입니다.";
            return;
        }

        _repository.Register(id, password);
        GotoLogin();
        _messageText.text = "가입이 완료되었습니다. 로그인 해주세요.";
    }
}
