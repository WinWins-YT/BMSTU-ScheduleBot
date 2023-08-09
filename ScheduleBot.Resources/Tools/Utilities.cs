namespace ScheduleBot.Resources.Tools
{
    /// <summary>
    /// Класс с общими утилитами
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// Конвертер номера пары во время начала пары
        /// </summary>
        /// <param name="para">Номер пары</param>
        /// <returns>Время начала пары</returns>
        internal static TimeOnly ParaToStartTime(int para)
        {
            return para switch
            {
                1 => new TimeOnly(8, 30),
                2 => new TimeOnly(10, 20),
                3 => new TimeOnly(12, 10),
                4 => new TimeOnly(14, 15),
                5 => new TimeOnly(16, 5),
                6 => new TimeOnly(17, 50),
                7 => new TimeOnly(19, 35),
                _ => new TimeOnly()
            };
        }
    }
}