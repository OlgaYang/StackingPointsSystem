namespace StakingPointsSystem.Services;

public static class MyDateTimeExtension
{
    public static DateTime TrimMilliseconds(this DateTime dateTime)
    {
        return dateTime.AddMilliseconds(-dateTime.Millisecond);
    }
}