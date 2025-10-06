using System;

namespace InLap.Api.DTOs
{
    public class ReportDto
    {
        public Guid UploadId { get; set; }
        public string SummaryJson { get; set; } = string.Empty;
        public string Article { get; set; } = string.Empty;
        public string LlmRaw { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
