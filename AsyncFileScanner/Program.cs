using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncFileScanner.Models;
using AsyncFileScanner.Services;

Console.WriteLine("=== FILE SCANNER ===\n");

Console.WriteLine("Введите путь к директории:");
string directoryPath = Console.ReadLine()?.Trim() ?? string.Empty;

Console.WriteLine("\nВведите расширения через запятую (например: .cs,.txt,.json):");
string extensionsInput = Console.ReadLine()?.Trim() ?? string.Empty;

// Нормализуем расширения: убираем пробелы, приводим к нижнему регистру, добавляем точку, если её нет
var extensions = extensionsInput
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(ext => ext.StartsWith('.') ? ext.ToLowerInvariant() : $".{ext.ToLowerInvariant()}")
    .ToList();

var options = new ScanOptions(directoryPath, extensions);
var probe = new FileProbe();
var scanner = new FileScanner(probe);

using var cts = new CancellationTokenSource();

Console.WriteLine("\nСканирование...");
Console.WriteLine("Нажмите Enter для отмены.\n");

// Запускаем фоновую задачу, которая ожидает нажатия Enter для отмены
var cancelTask = Task.Run(() =>
{
    Console.ReadLine();
    cts.Cancel();
});

try
{
    var summary = await scanner.ScanAsync(options, cts.Token);

    if (cts.Token.IsCancellationRequested)
    {
        Console.WriteLine("\nСканирование отменено пользователем.");
        Console.WriteLine($"Обработано файлов: {summary.SuccessfulFiles}");
    }
    else
    {
        Console.WriteLine("\nНайденные файлы:\n");

        for (int i = 0; i < summary.Results.Count; i++)
        {
            var res = summary.Results[i];
            // Форматируем вывод: имя файла выравнивается по левому краю (ширина 25 символов)
            Console.WriteLine($"[{i + 1}] {res.FileName,-25} {res.SizeInBytes} байт");
        }

        Console.WriteLine("\nСканирование завершено.");
        Console.WriteLine($"\nВсего найдено файлов: {summary.SuccessfulFiles}");
        Console.WriteLine($"Общий размер: {summary.TotalBytes} байт");

        if (summary.Errors.Count > 0)
        {
            Console.WriteLine($"\n Произошло ошибок: {summary.Errors.Count}");
            // Выводим первые 3 ошибки для примера, чтобы не засорять консоль
            foreach (var error in summary.Errors.Take(3))
            {
                Console.WriteLine($"  - {error}");
            }
            if (summary.Errors.Count > 3)
            {
                Console.WriteLine($"  ... и ещё {summary.Errors.Count - 3} ошибок.");
            }
        }
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nСканирование отменено пользователем.");
}
catch (Exception ex)
{
    Console.WriteLine($"\n Критическая ошибка: {ex.Message}");
}