namespace Application.Exceptions
{
    public class ThirdPartyException : Exception
    {
        public ThirdPartyException(string error)
            : base(error) { }
    }
}
