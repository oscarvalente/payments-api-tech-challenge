public interface IToken
{
    public string verifyToken(string jwtToken);
    public string createToken(string username);
}