using System;

namespace InLap.Infrastructure.Persistence.Entities
{
    public class UploadRecord
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string StoredPath { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public ReportRecord? Report { get; set; }
    }
}
