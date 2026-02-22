namespace Application.Exceptions
{
    public class NotActiveException : Exception
    {
        public NotActiveException(string message)
            : base($"\"{message}\" was not found or is expired or is not active") { }
    }
}
