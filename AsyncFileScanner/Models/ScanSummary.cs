namespace AsyncFileScanner.Models;

/// <summary>
/// Здесь хранятся общие результаты сканирования.
/// </summary>
public sealed record ScanSummary(
    int ScannedFiles,
    int SuccessfulFiles,
    long TotalBytes,
    TimeSpan ElapsedTime,
    IReadOnlyList<ScanResult> Results,
    IReadOnlyList<string> Errors);