using System;
using System.Linq;
using System.Threading.Tasks;
using InLap.Api.DTOs;
using InLap.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InLap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly InLapDbContext _db;

        public ReportsController(InLapDbContext db)
        {
            _db = db;
        }

        [HttpGet("{uploadId:guid}")]
        [ProducesResponseType(typeof(ReportDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ReportDto>> Get(Guid uploadId)
        {
            var report = await _db.Reports.AsNoTracking()
                .Where(r => r.UploadId == uploadId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .FirstOrDefaultAsync();

            if (report == null) return NotFound();

            var dto = new ReportDto
            {
                UploadId = report.UploadId,
                SummaryJson = report.SummaryJson,
                Article = report.Article,
                LlmRaw = report.LlmRaw,
                CreatedAtUtc = report.CreatedAtUtc
            };

            return Ok(dto);
        }
    }
}
