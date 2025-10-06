using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace InLap.App.Interfaces
{
    public interface IFileStore
    {
        /// <summary>
        /// Saves a file stream enforcing extension and size validations.
        /// Returns the generated uploadId and the stored absolute file path.
        /// </summary>
        /// <param name="originalFileName">Original client-provided file name.</param>
        /// <param name="content">File stream to save. Caller remains owner of the stream.</param>
        /// <param name="maxBytes">Maximum allowed size in bytes.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<(System.Guid uploadId, string storedPath)> SaveAsync(string originalFileName, Stream content, long maxBytes = 1_000_000, CancellationToken ct = default);
    }
}
