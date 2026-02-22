namespace Application.Exceptions
{
    public class CannotUpdateException : Exception
    {
        public CannotUpdateException(string property)
            : base($"\"{property}\" cannot be update") { }

        public CannotUpdateException(string property, string reason)
            : base($"\"{property}\" cannot be update because " + reason) { }
    }
}
