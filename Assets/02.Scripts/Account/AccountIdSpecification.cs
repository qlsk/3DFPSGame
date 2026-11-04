using System.Text.RegularExpressions;

public class AccountIdSpecification
{
    // 아이디: 이메일 형식이거나, 영문·숫자·-·@·_로 이루어진 3~20 글자
    private const string IdPattern = @"^(?:[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}|[A-Za-z0-9\-@_]{3,20})$";

    public string ErrorMessage => "아이디는 이메일 형식이어야 합니다";
    public bool IsSatisfiedBy(string email)
    {
        return Regex.IsMatch(email, IdPattern);
    }
}
