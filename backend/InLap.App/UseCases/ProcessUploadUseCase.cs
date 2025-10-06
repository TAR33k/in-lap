using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InLap.App.DTOs;
using InLap.App.Interfaces;
using InLap.App.Summary;

namespace InLap.App.UseCases
{
    public class ProcessUploadUseCase
    {
        private readonly IFileStore _fileStore;
        private readonly IFileParsingService _parsing;
        private readonly ISummaryService _summary;
        private readonly ILLMClient _llm;
        private readonly ResponseCleaner _cleaner;
        private readonly IReportRepository _repo;

        public ProcessUploadUseCase(
            IFileStore fileStore,
            IFileParsingService parsing,
            ISummaryService summary,
            ILLMClient llm,
            ResponseCleaner cleaner,
            IReportRepository repo)
        {
            _fileStore = fileStore;
            _parsing = parsing;
            _summary = summary;
            _llm = llm;
            _cleaner = cleaner;
            _repo = repo;
        }

        public async Task<Guid> ExecuteAsync(string originalFileName, Stream csvStream, CancellationToken ct = default)
        {
            if (csvStream == null) throw new ArgumentNullException(nameof(csvStream));
            if (string.IsNullOrWhiteSpace(originalFileName)) throw new ArgumentException("File name is required", nameof(originalFileName));

            await using var buffer = new MemoryStream();
            await csvStream.CopyToAsync(buffer, ct);
            buffer.Seek(0, SeekOrigin.Begin);

            var (uploadId, storedPath) = await _fileStore.SaveAsync(originalFileName, buffer, ct: ct);

            buffer.Seek(0, SeekOrigin.Begin);
            var (weekend, warnings) = await _parsing.ParseAsync(buffer, ct);

            var summaryDto = _summary.Compose(weekend, warnings);
            var summaryJson = JsonSerializer.Serialize(summaryDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var systemPrompt = "You are a motorsport journalist. Input: a single JSON object summary. Rules: Use ONLY facts inside summary. Do NOT invent, infer, or add facts. If a fact is missing, omit it. Do not guess. Output plain text only, with these parts in this order: 1) HEADLINE — one line, max 10 words. 2) LEAD — 1–2 sentences that state the main point. 3) BODY — 2–4 short paragraphs, total 80–160 words. 4) QUICK FACTS — 3–6 bullet points showing exact values from summary. Tone: neutral, factual, concise. Use active voice and third person. Do not add quotes unless summary contains them. Do not output metadata, explanations, or sources. If summary is missing or invalid JSON, output exactly: ERROR: missing or invalid JSON summary.";
            var userPrompt = summaryJson;
            var llmRaw = await _llm.CompleteAsync(userPrompt, systemPrompt, ct);
            var cleaned = _cleaner.Clean(llmRaw, summaryDto);

            await _repo.SaveUploadAsync(uploadId, originalFileName, storedPath, DateTime.UtcNow, ct);
            await _repo.SaveReportAsync(Guid.NewGuid(), uploadId, summaryJson, llmRaw, cleaned, DateTime.UtcNow, ct);

            return uploadId;
        }
    }
}
