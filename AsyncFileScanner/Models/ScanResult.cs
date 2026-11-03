namespace AsyncFileScanner.Models;

/// <summary>
/// Модель результата обработки одного файла.
/// </summary>
public sealed record ScanResult(
    string FilePath,
    string FileName,
    long SizeInBytes,
    int HeaderBytesRead);