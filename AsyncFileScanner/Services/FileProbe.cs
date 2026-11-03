using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncFileScanner.Services;

/// <summary>
/// Сервис для асинхронного чтения заголовка файла небольшими блоками.
/// </summary>
public sealed class FileProbe
{
    public async Task<int> ReadHeaderAsync(
        string path,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];

        // Открываем файл асинхронно с правом только на чтение и общим доступом на чтение для других процессов
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true); // FileOptions.Asynchronous

        // Читаем первые 1024 байта (или меньше, если файл меньше)
        return await stream.ReadAsync(
            buffer.AsMemory(),
            cancellationToken);
    }
}