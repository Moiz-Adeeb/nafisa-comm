namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException()
            : base($"Resource was not found") { }

        public NotFoundException(string name, object key = null)
            : base($"Resource \"{name}\" was not found") { }
    }
}
