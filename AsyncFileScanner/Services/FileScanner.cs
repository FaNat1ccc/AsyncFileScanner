using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AsyncFileScanner.Models;

namespace AsyncFileScanner.Services;

/// <summary>
/// Сервис сканирования директории.
/// </summary>
public sealed class FileScanner
{
    private readonly FileProbe _probe;

    public FileScanner(FileProbe probe)
    {
        _probe = probe;
    }

    public async Task<ScanSummary> ScanAsync(
        ScanOptions options,
        CancellationToken cancellationToken)
    {
        ValidateOptions(options);

        var stopwatch = Stopwatch.StartNew();
        var results = new List<ScanResult>();
        var errors = new List<string>();

        int scannedFiles = 0;
        long totalBytes = 0;

        // Directory.EnumerateFiles выполняется синхронно, но возвращает данные лениво (по одному), 
        // что позволяет не загружать всю структуру файлов в память сразу.
        var allFiles = Directory.EnumerateFiles(options.DirectoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var filePath in allFiles)
        {
            // Проверяем отмену перед обработкой каждого файла
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            // Фильтруем по расширениям (игнорируем регистр)
            if (!options.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            scannedFiles++;

            try
            {
                // Получаем размер файла без удержания открытого дескриптора
                var fileInfo = new FileInfo(filePath);
                long size = fileInfo.Length;
                totalBytes += size;

                // Асинхронно читаем заголовок
                int headerBytesRead = await _probe.ReadHeaderAsync(filePath, cancellationToken);

                results.Add(new ScanResult(
                    filePath,
                    Path.GetFileName(filePath),
                    size,
                    headerBytesRead));
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add($"[Доступ запрещён] {filePath}: {ex.Message}");
            }
            catch (IOException ex)
            {
                errors.Add($"[Ошибка ввода-вывода] {filePath}: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"[Неизвестная ошибка] {filePath}: {ex.Message}");
            }
        }

        stopwatch.Stop();

        return new ScanSummary(
            scannedFiles,
            results.Count,
            totalBytes,
            stopwatch.Elapsed,
            results,
            errors);
    }

    private static void ValidateOptions(ScanOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DirectoryPath))
        {
            throw new ArgumentException("Путь к директории не может быть пустым.", nameof(options));
        }

        if (!Directory.Exists(options.DirectoryPath))
        {
            throw new DirectoryNotFoundException($"Указанная директория не существует: {options.DirectoryPath}");
        }

        if (options.Extensions == null || options.Extensions.Count == 0)
        {
            throw new ArgumentException("Список расширений не может быть пустым.", nameof(options));
        }
    }
}