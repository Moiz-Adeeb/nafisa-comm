namespace Application.Exceptions
{
    public class EntityIsLockedException : Exception
    {
        public EntityIsLockedException()
            : base("Entity is locked hence cannot be changed") { }

        public EntityIsLockedException(string entity)
            : base($"{entity} is locked hence cannot be changed") { }
    }
}
