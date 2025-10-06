using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.Interfaces;
using InLap.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InLap.Infrastructure.FileStorage
{
    public class LocalFileStore : IFileStore
    {
        private readonly InfrastructureOptions _options;

        public LocalFileStore(IOptions<InfrastructureOptions> options)
        {
            _options = options.Value ?? new InfrastructureOptions();
        }

        public async Task<(Guid uploadId, string storedPath)> SaveAsync(string originalFileName, Stream content, long maxBytes = 1_000_000, CancellationToken ct = default)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("Original file name is required.", nameof(originalFileName));

            var sizeCap = _options.MaxUploadBytes > 0 ? _options.MaxUploadBytes : maxBytes;

            var ext = Path.GetExtension(originalFileName)?.ToLowerInvariant();
            if (!string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only .csv files are allowed.");
            }

            var basePath = _options.FilesBasePath;
            if (string.IsNullOrWhiteSpace(basePath))
            {
                basePath = "file-store";
            }
            if (!Path.IsPathRooted(basePath))
            {
                basePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, basePath));
            }

            Directory.CreateDirectory(basePath);

            var uploadId = Guid.NewGuid();
            var safeFileName = uploadId.ToString("N") + ".csv";
            var fullPath = Path.Combine(basePath, safeFileName);

            const int bufferSize = 81920; // 80KB
            long total = 0;
            await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
            {
                var buffer = new byte[bufferSize];
                int read;
                while ((read = await content.ReadAsync(buffer.AsMemory(0, buffer.Length), ct)) > 0)
                {
                    total += read;
                    if (total > sizeCap)
                    {
                        try { fs.Dispose(); File.Delete(fullPath); } catch { /* ignore */ }
                        throw new InvalidOperationException($"File exceeds maximum allowed size of {sizeCap} bytes.");
                    }
                    await fs.WriteAsync(buffer.AsMemory(0, read), ct);
                }
            }

            if (total == 0)
            {
                try { File.Delete(fullPath); } catch { /* ignore */ }
                throw new InvalidOperationException("Uploaded file is empty.");
            }

            return (uploadId, fullPath);
        }
    }
}
