namespace Application.Exceptions
{
    public class TokenExpiredException : Exception
    {
        public TokenExpiredException()
            : base("Token is Expired") { }
    }

    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException()
            : base("Token not found") { }
    }
}
