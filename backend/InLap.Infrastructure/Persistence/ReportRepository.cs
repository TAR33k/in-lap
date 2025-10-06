using System;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.Interfaces;
using InLap.Infrastructure.Persistence.Entities;

namespace InLap.Infrastructure.Persistence
{
    public class ReportRepository : IReportRepository
    {
        private readonly InLapDbContext _db;

        public ReportRepository(InLapDbContext db)
        {
            _db = db;
        }

        public async Task SaveUploadAsync(Guid uploadId, string fileName, string storedPath, DateTime createdAtUtc, CancellationToken ct = default)
        {
            var upload = new UploadRecord
            {
                Id = uploadId,
                FileName = fileName,
                StoredPath = storedPath,
                CreatedAtUtc = createdAtUtc
            };

            _db.Uploads.Add(upload);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SaveReportAsync(Guid reportId, Guid uploadId, string summaryJson, string llmRaw, string article, DateTime createdAtUtc, CancellationToken ct = default)
        {
            var report = new ReportRecord
            {
                Id = reportId,
                UploadId = uploadId,
                SummaryJson = summaryJson,
                LlmRaw = llmRaw,
                Article = article,
                CreatedAtUtc = createdAtUtc
            };

            _db.Reports.Add(report);
            await _db.SaveChangesAsync(ct);
        }
    }
}
