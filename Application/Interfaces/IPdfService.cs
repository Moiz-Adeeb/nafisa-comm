namespace Application.Interfaces;

public interface IPdfService
{
    byte[] WriteRows<T>(
        List<T> data,
        string header = "Report",
        Dictionary<string, string> details = null
    );

    byte[] GenerateReport(Dictionary<string, bool> equipments, string shift, string date);
}
