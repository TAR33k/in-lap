using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InLap.Api.DTOs;
using InLap.App.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InLap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ProcessUploadUseCase _useCase;

        public UploadController(ProcessUploadUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1_000_000)]
        [ProducesResponseType(typeof(UploadResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        public async Task<ActionResult<UploadResponseDto>> Upload(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            var fileName = file.FileName ?? "upload.csv";
            var ext = Path.GetExtension(fileName);
            if (!string.Equals(ext, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only .csv files are allowed.");
            }

            await using var stream = file.OpenReadStream();
            var uploadId = await _useCase.ExecuteAsync(fileName, stream, ct);
            return Ok(new UploadResponseDto { UploadId = uploadId });
        }
    }
}
