namespace Application.Interfaces
{
    public interface ISessionService
    {
        string GetTimeZone();
        string GetUserId();
        string GetChatId();
        string GetCompanyId();
        string GetRole();
        bool HasRole(params string[] roles);
        string GetIpAddress();
        string[] GetGroupIds();
    }

    public interface IAlertService
    {
        Task<bool> SendNotificationToAdmin(string title, string message, string type = "alert");
        Task<bool> SendNotificationToUser(
            string userId,
            string title,
            string message,
            string type = "alert"
        );
        Task<bool> SendNotificationToRole(
            string role,
            string title,
            string message,
            string type = "alert"
        );
    }
}
