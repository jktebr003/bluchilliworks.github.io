namespace MudBlazorWeb.Shared.Extensions;

public static class DateTimeExtension
{
   public static string GetDurationDate(string dateStart, string dateEnd)
    {
        if (dateEnd != null && dateEnd.Length > 0)
        {
            DateTime start = DateTime.Parse(dateStart);
            DateTime end = DateTime.Parse(dateEnd);

            return $"{start.Date:dd MMMM yyyy} - {end.Date:dd MMMM yyyy}";
        }
        else
        {
            DateTime start = DateTime.Parse(dateStart);

            return $"{start.Date:dd MMMM yyyy} - current";
        }
    }

    public static DateTime GetSouthAfricanTime()
    {
        var SoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");
        DateTime SoTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.Local, SoTimeZone);

        return SoTime;
    }

    public static DateTime? ToSouthAfricanDateTime(this string? input)
    {
        DateTime date = DateTime.Now;
        if (DateTime.TryParse(input, out date) && input != null)
        {
            var SoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Johannesburg");
            DateTime SoTime = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, SoTimeZone);

            return SoTime;
        }

        return null;
    }
}
