namespace Application.Exceptions
{
    public class OkayButNotSuccessfulException : Exception
    {
        public OkayButNotSuccessfulException() { }

        public OkayButNotSuccessfulException(string message)
            : base(message) { }

        public OkayButNotSuccessfulException NotFound(string objName)
        {
            return new OkayButNotSuccessfulException($"\"{objName}\" was not found");
        }
    }
}
