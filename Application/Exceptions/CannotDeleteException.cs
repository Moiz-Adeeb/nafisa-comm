namespace Application.Exceptions
{
    public class CannotDeleteException : Exception
    {
        public CannotDeleteException(string name)
            : base($"Resource \"{name}\" cannot be deleted as its already in use or doesn't exist")
        { }

        public CannotDeleteException()
            : base($"Resource cannot be deleted as its already in use or doesn't exist") { }
    }
}
