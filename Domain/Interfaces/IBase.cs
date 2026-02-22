namespace Domain.Interfaces
{
    public interface IBase
    {
        /// <summary>
        /// The Date it was created
        /// </summary>
        DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The Date it was Updated
        /// </summary>
        DateTimeOffset UpdatedDate { get; set; }
        bool IsDeleted { get; set; }
    }
}
