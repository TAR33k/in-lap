using System;
using System.Threading;
using System.Threading.Tasks;

namespace InLap.App.Interfaces
{
    public interface IReportRepository
    {
        /// <summary>
        /// Saves an upload record.
        /// </summary>
        /// <param name="uploadId">The upload ID.</param>
        /// <param name="fileName">The original file name.</param>
        /// <param name="storedPath">The stored file path.</param>
        /// <param name="createdAtUtc">The creation time in UTC.</param>
        /// <param name="ct">Cancellation token.</param>
        Task SaveUploadAsync(Guid uploadId, string fileName, string storedPath, DateTime createdAtUtc, CancellationToken ct = default);

        /// <summary>
        /// Saves a report record.
        /// </summary>
        /// <param name="reportId">The report ID.</param>
        /// <param name="uploadId">The upload ID.</param>
        /// <param name="summaryJson">The summary JSON.</param>
        /// <param name="llmRaw">The raw LLM response.</param>
        /// <param name="article">The article text.</param>
        /// <param name="createdAtUtc">The creation time in UTC.</param>
        /// <param name="ct">Cancellation token.</param>
        Task SaveReportAsync(Guid reportId, Guid uploadId, string summaryJson, string llmRaw, string article, DateTime createdAtUtc, CancellationToken ct = default);
    }
}
