namespace ScheduleBot.Resources.Extensions;

/// <summary>
/// Класс с расширениями для строки
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Ковертер строки в число с плавающей запятой
    /// </summary>
    /// <param name="s">Строка для конвертации</param>
    /// <returns>Число из строки</returns>
    public static double ToDouble(this string s)
        => Convert.ToDouble(s);
}