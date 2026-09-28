using System;
using System.Globalization;

namespace Atlassian.Jira;

public static class FormatUtils
{
    public const string DEFAULT_DATE_FORMAT = "yyyy/MM/dd";
    public const string DEFAULT_DATE_TIME_FORMAT = DEFAULT_DATE_FORMAT + " HH:mm";
    public static readonly CultureInfo DefaultCultureInfo = CultureInfo.GetCultureInfo("en-us");

    internal static string FormatDateTimeString(DateTime value)
    {
        /* Using "en-us" culture to conform to formats of JIRA.
         * See https://bitbucket.org/farmas/atlassian.net-sdk/issue/31
         */
        return value.ToString(
            value.TimeOfDay == TimeSpan.Zero ? DEFAULT_DATE_FORMAT : DEFAULT_DATE_TIME_FORMAT,
            DefaultCultureInfo);
    }
}