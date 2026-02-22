using Domain.Interfaces;

namespace Domain.Entities
{
    public class Base : IBase
    {
        public Base()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        public string Id { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset UpdatedDate { get; set; }
        public bool IsDeleted { get; set; }
    }
    public class BaseWithUser : Base
    {
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
