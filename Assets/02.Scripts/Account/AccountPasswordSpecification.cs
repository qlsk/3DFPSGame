using System.Text.RegularExpressions;

public class AccountPasswordSpecification
{
    // 비밀번호: 공백 없는 영문·숫자·기호 8~30 글자, 대문자·소문자·숫자·기호를 각각 하나 이상 포함
    // (?=.*X)는 전방 탐색으로, 문자열 어딘가에 X가 있는지만 확인하고 위치는 옮기지 않는다.
    private const string PasswordPattern = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9])(?=.*[^A-Za-z0-9])[\x21-\x7E]{8,30}$";

    public string ErrorMessage => "비밀번호는 대문자, 소문자, 숫자, 기호를 포함한 8~30 글자여야 합니다";
    public bool IsSatisfiedBy(string password)
    {
        return Regex.IsMatch(password, PasswordPattern);
    }
}
