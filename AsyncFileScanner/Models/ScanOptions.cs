namespace AsyncFileScanner.Models;

/// <summary>
/// Модель содержит настройки сканирования.
/// </summary>
public sealed record ScanOptions(
    string DirectoryPath,
    IReadOnlyList<string> Extensions);