using Avalonia;
using Avalonia.Skia;
using System;

namespace SortingAlgorithms.GUI;

class Program
{
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UseSkia()
            .UsePlatformDetect() // Оставляем это!
            .LogToTrace();
}