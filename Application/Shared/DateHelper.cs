namespace Application.Shared;

public class DateHelper
{
    public static DateOnly FindUpComingDate(
        DateOnly joiningDate,
        DateOnly? lastSaleDate,
        int monthsToAdd,
        DateTime now
    )
    {
        // if (lastSaleDate != null && )
        // {
        //     now = lastSaleDate.Value.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero));
        // }
        var date = (lastSaleDate ?? joiningDate).ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.Zero));
        while (date < now)
        {
            date = date.AddMonths(monthsToAdd);
        }
        return new DateOnly(date.Year, date.Month, date.Day);
    }
}
