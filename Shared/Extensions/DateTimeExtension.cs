using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Extensions;

public static class DateTimeExtension
{
    //public static string ToDatePosted(this DateTime datePosted)
    //{
    //    TimeSpan ts = DateTime.Now - datePosted;

    //    if (ts.TotalMinutes < 2)
    //        return "Just now";
    //    else if (ts.TotalMinutes < 60)
    //        return $"{ts.Minutes}m ago";
    //    else if (ts.TotalHours < 24)
    //        return $"{ts.Hours}h ago";
    //    else if (ts.TotalDays < 365)
    //        return datePosted.Date.ToString("dd MMMM");
    //    else
    //        return datePosted.Date.ToString("dd MMMM yyyy");
    //}

    //public static string ToDateCommented(this string dateCommented)
    //{
    //    TimeSpan ts = DateTime.Now - DateTime.Parse(dateCommented);

    //    if (ts.TotalMinutes < 2)
    //        return "Just now";
    //    else if (ts.TotalMinutes < 60)
    //        return $"{ts.Minutes}m ago";
    //    else if (ts.TotalHours < 24)
    //        return $"{ts.Hours}h ago";
    //    else if (ts.TotalDays < 365)
    //        return DateTime.Parse(dateCommented).Date.ToString("dd MMMM");
    //    else
    //        return DateTime.Parse(dateCommented).Date.ToString("dd MMMM yyyy");
    //}

    //public static string GetDurationDate(DateTime dateStart, DateTime dateEnd)
    //{
    //    return $"{dateStart.Date:dd MMMM yyyy} - {dateEnd.Date:dd MMMM yyyy}";
    //}

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
