using System.Diagnostics;
using System.Runtime.InteropServices;
using ScheduleBot.Resources.Extensions;

namespace ScheduleBot.Resources.Tools;

/// <summary>
/// Класс для получения системной информации
/// </summary>
internal class SystemLoad
{
    /// <summary>
    /// Свободный объем ОЗУ
    /// </summary>
    internal double FreeMemory { get; }
    
    /// <summary>
    /// Полный объем ОЗУ
    /// </summary>
    internal double TotalMemory { get; }
    
    /// <summary>
    /// Занятый объем ОЗУ
    /// </summary>
    internal double UsedMemory { get; }
    
    /// <summary>
    /// Процент загрузки ЦП
    /// </summary>
    internal double CpuLoad { get; }
    
    /// <summary>
    /// Модель процессора
    /// </summary>
    internal string CpuModel { get; }
    
    /// <summary>
    /// Время работы системы
    /// </summary>
    internal TimeSpan UpTime { get; }
    
    /// <summary>
    /// Конструктор класса
    /// </summary>
    internal SystemLoad()
    {
        if (!IsUnix())
        {
            string output;
            var info = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "OS get FreePhysicalMemory,TotalVisibleMemorySize /Value",
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }

            var lines = output.Trim().Split("\n");
            FreeMemory = lines[0].Split('=', StringSplitOptions.RemoveEmptyEntries)[1].ToDouble() / 1024;
            TotalMemory = lines[1].Split('=', StringSplitOptions.RemoveEmptyEntries)[1].ToDouble() / 1024;
            UsedMemory = TotalMemory - FreeMemory;

            info.Arguments = "cpu get loadpercentage /Value";

            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }

            lines = output.Trim().Split('\n');
            CpuLoad = lines[0].Split('=', StringSplitOptions.RemoveEmptyEntries)[1].ToDouble();

            info.Arguments = "cpu get name /Value";
            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }
            CpuModel = output.Split('=', StringSplitOptions.RemoveEmptyEntries)[1].Trim();
            DateTime systemStartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount);
            UpTime = DateTime.Now - systemStartTime;
        }
        else
        {
            string output;

            var info = new ProcessStartInfo("free -m")
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"",
                RedirectStandardOutput = true
            };

            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }

            var lines = output.Split("\n");
            var memory = lines[1].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            TotalMemory = memory[1].ToDouble();
            UsedMemory = memory[2].ToDouble();
            FreeMemory = memory[3].ToDouble();

            info.FileName = "/bin/bash";
            info.Arguments = "-c \"top -bn1 | grep \\\"Cpu(s)\\\" | sed \\\"s/.* \\\\([0-9,.]*\\\\) id.*/\\\\1/\\\" | awk '{print 100 - $1}'\"";

            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }

            CpuLoad = output.Trim().ToDouble();

            info.FileName = "/bin/bash";
            info.Arguments = "-c \"grep -m 1 'model name' /proc/cpuinfo\"";

            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }

            CpuModel = output.Split(':', StringSplitOptions.RemoveEmptyEntries)[1].Trim();

            info.FileName = "/bin/bash";
            info.Arguments = "-c \"uptime -s\"";
            using (var process = Process.Start(info))
            {
                output = process is not null ? process.StandardOutput.ReadToEnd() : "0";
            }
            var upSince = DateTime.ParseExact(output.Trim(), "yyyy-MM-dd HH:mm:ss", new System.Globalization.CultureInfo("en-us"));
            UpTime = DateTime.Now.Subtract(upSince);
        }
    }

    private static bool IsUnix()
        => RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
           RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
}