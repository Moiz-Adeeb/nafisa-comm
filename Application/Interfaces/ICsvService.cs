namespace Application.Interfaces;

public interface ICsvService
{
    byte[] WriteRows<T>(List<T> data);
}
