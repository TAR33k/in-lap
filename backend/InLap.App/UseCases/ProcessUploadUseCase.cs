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

            var systemPrompt = string.Join(" ", new[]
            {
                "You are a motorsport journalist writing for a sim racing league audience.",
                "Input: a single JSON object summary.",
                "Rules: Use ONLY facts inside summary. Do NOT invent, infer, or add facts. If a fact is missing, omit it. Do not guess.",
                "Chronology: Follow the sessions array order exactly: Practice, Qualify, Race1, Race2.",

                "Output plain text only, with these parts in this order:",
                "1) HEADLINE — punchy, 10–15 words, action-oriented.",
                "2) LEAD — 2–3 energetic sentences that set the story.",
                "3) BODY — 5–7 short paragraphs (130–230 words total) that highlight winners, best laps, and the biggest incidents (use drivers and exact speeds). Start with Qualify to set the grid and tone, then Race 1 and Race 2.",
                "4) QUICK FACTS — 4–8 bullet points showing exact values from summary.",

                "Style: lively, engaging, and fun but accurate. Keep it respectful. Vary sentence length. Use strong verbs. Avoid clichés and filler.",
                "Mention notable incidents by naming drivers and showing their impact speeds (rounded numbers with km/h) where available. Impact with environment should be written as impact with a wall or hit the wall.",

                "Formatting: Keep clear section breaks and bullets exactly as specified.",
                "Use active voice and third person. Do not add quotes unless summary contains them. Do not output JSON, code fences, or metadata.",
                "If summary is missing or invalid JSON, output exactly: ERROR: missing or invalid JSON summary."
            });
            var userPrompt = summaryJson;
            var llmRaw = await _llm.CompleteAsync(userPrompt, systemPrompt, ct);
            var cleaned = _cleaner.Clean(llmRaw, summaryDto);

            await _repo.SaveUploadAsync(uploadId, originalFileName, storedPath, DateTime.UtcNow, ct);
            await _repo.SaveReportAsync(Guid.NewGuid(), uploadId, summaryJson, llmRaw, cleaned, DateTime.UtcNow, ct);

            return uploadId;
        }
    }
}
