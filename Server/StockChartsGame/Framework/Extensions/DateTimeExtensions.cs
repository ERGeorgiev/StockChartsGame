namespace StockChartsGame.Framework.Components;

public static class DateTimeExtensions
{
    public static int TotalDays(this DateTime value)
    {
        return (int)(value - new DateTime()).TotalDays;
    }
}
