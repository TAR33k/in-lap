using System;

namespace InLap.Infrastructure.Persistence.Entities
{
    public class ReportRecord
    {
        public Guid Id { get; set; }

        public Guid UploadId { get; set; }

        public string SummaryJson { get; set; } = string.Empty;

        public string LlmRaw { get; set; } = string.Empty;

        public string Article { get; set; } = string.Empty;
        
        public DateTime CreatedAtUtc { get; set; }

        public UploadRecord? Upload { get; set; }
    }
}
